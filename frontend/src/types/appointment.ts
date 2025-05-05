export interface Appointment {
  id: number;
  patientName: string;
  doctorName: string;
  date: string;
  time: string;
  status: 'scheduled' | 'completed' | 'cancelled';
  description?: string;
  duration?: number; // Added duration field
  suggestedTimes?: string[];
}

export interface AppointmentFormData {
  patientName: string;
  doctorName: string;
  date: string;
  time: string;
  description?: string;
  duration?: number; // Added duration field
}

export interface CreateAppointmentResponse {
  success: boolean;
  suggestedTimes?: string[];
}