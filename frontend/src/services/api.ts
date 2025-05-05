import axios from 'axios';
import { Appointment, AppointmentFormData, CreateAppointmentResponse } from '../types/appointment';

const API_BASE_URL = 'http://localhost:5077/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add logging for requests and responses
api.interceptors.request.use((config) => {
  console.log('Request:', config);
  return config;
}, (error) => {
  console.error('Request error:', error);
  return Promise.reject(error);
});

api.interceptors.response.use((response) => {
  console.log('Response:', response);
  return response;
}, (error) => {
  console.error('Response error:', error);
  return Promise.reject(error);
});

// Modify transformAppointment to avoid converting UTC to local time
const transformAppointment = (data: any): Appointment => {
  if (!data.appointmentDate) {
    console.error('Missing or invalid appointmentDate in API response:', data);
    return {
      id: data.id || 0,
      patientName: data.patientName || 'Unknown',
      doctorName: data.healthcareProfessionalName || 'Unknown',
      date: 'Invalid Date',
      time: 'Invalid Time',
      status: 'scheduled',
      description: data.description || '',
      duration: data.duration || 30,
    };
  }

  const utcDate = new Date(data.appointmentDate);
  if (isNaN(utcDate.getTime())) {
    console.error('Invalid appointmentDate format:', data.appointmentDate);
    return {
      id: data.id || 0,
      patientName: data.patientName || 'Unknown',
      doctorName: data.healthcareProfessionalName || 'Unknown',
      date: 'Invalid Date',
      time: 'Invalid Time',
      status: 'scheduled',
      description: data.description || '',
      duration: data.duration || 30,
    };
  }

  // Use UTC date and time directly without conversion
  const formattedDate = utcDate.toISOString().split('T')[0];
  const formattedTime = utcDate.toISOString().split('T')[1].slice(0, 5); // Extract HH:mm

  return {
    id: data.id,
    patientName: data.patientName,
    doctorName: data.healthcareProfessionalName,
    date: formattedDate,
    time: formattedTime,
    status: 'scheduled',
    description: data.description,
    duration: data.duration || 30,
  };
};

export const appointmentService = {
  getAll: async (): Promise<Appointment[]> => {
    try {
      console.log('Fetching appointments...');
      const response = await api.get('/appointments');
      console.log('Raw API Response:', response.data);
      const transformed = response.data.map(transformAppointment);
      console.log('Transformed appointments:', transformed);
      return transformed.sort((a: Appointment, b: Appointment) => {
        const dateA = new Date(`${a.date}T${a.time}`);
        const dateB = new Date(`${b.date}T${b.time}`);
        return dateA.getTime() - dateB.getTime();
      }); // Sort by date and time
    } catch (error) {
      console.error('Error fetching appointments:', error);
      if (axios.isAxiosError(error)) {
        console.error('Response data:', error.response?.data);
        console.error('Response status:', error.response?.status);
      }
      throw error;
    }
  },

  getById: async (id: number): Promise<Appointment> => {
    try {
      const response = await api.get(`/appointments/${id}`);
      return transformAppointment(response.data);
    } catch (error) {
      console.error(`Error fetching appointment ${id}:`, error);
      throw error;
    }
  },

  create: async (data: AppointmentFormData): Promise<CreateAppointmentResponse> => {
    try {
      console.log('Creating appointment with data:', data);

      const backendData = {
        patientName: data.patientName,
        healthcareProfessionalName: data.doctorName,
        appointmentDate: `${data.date}T${data.time}`, // Send as local date-time string
        duration: data.duration,
        description: data.description,
      };

      console.log('Sending to backend:', backendData);
      const response = await api.post('/appointments', backendData);
      console.log('Backend response:', response.data);

      return {
        success: response.data.success || false,
        suggestedTimes: response.data.suggestedTimes || [],
      };
    } catch (error) {
      console.error('Error creating appointment:', error);
      throw error;
    }
  },

  update: async (id: number, data: Partial<AppointmentFormData>): Promise<Appointment> => {
    try {
      const backendData: Record<string, any> = {}; // Use a generic object to map to backend fields
      if (data.patientName !== undefined) backendData.patientName = data.patientName;
      if (data.doctorName !== undefined) backendData.healthcareProfessionalName = data.doctorName;
      if (data.date && data.time) {
        backendData.appointmentDate = `${data.date}T${data.time}`; // Send as local date-time string
      }
      if (data.description !== undefined) backendData.description = data.description;
      if (data.duration !== undefined) backendData.duration = data.duration; // Ensure duration is included
      const response = await api.put(`/appointments/${id}`, backendData);

      if (response.data.success === false && Array.isArray(response.data.suggestedTimes)) {
        console.warn('Update failed. Suggested times:', response.data.suggestedTimes);
        const error = new Error('Update failed with suggested times.');
        (error as any).suggestedTimes = response.data.suggestedTimes; // Attach suggestedTimes to the error object
        throw error;
      }

      return transformAppointment(response.data);
    } catch (error) {
      console.error(`Error updating appointment ${id}:`, error);
      throw error;
    }
  },

  delete: async (id: number): Promise<void> => {
    try {
      await api.delete(`/appointments/${id}`);
    } catch (error) {
      console.error(`Error deleting appointment ${id}:`, error);
      throw error;
    }
  },

  search: async (term: string): Promise<Appointment[]> => {
    try {
      const response = await api.get(`/appointments/search`, { params: { query: term } });
      return response.data.map(transformAppointment);
    } catch (error) {
      console.error('Error searching appointments:', error);
      throw error;
    }
  },

  getDoctors: async (): Promise<string[]> => {
    try {
      const response = await api.get('/doctors'); // Assuming the endpoint exists
      return response.data
        .map((doctor: any) => doctor.name) // Map to extract doctor names
        .sort((a: string, b: string) => a.localeCompare(b)); // Sort by name
    } catch (error) {
      console.error('Error fetching doctors:', error);
      throw error;
    }
  },
};