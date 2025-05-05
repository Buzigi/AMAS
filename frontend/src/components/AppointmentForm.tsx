import { useState, useEffect } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { appointmentService } from '../services/api';
import { AppointmentFormData, CreateAppointmentResponse } from '../types/appointment';

const AppointmentForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEditing = Boolean(id);

  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const doctorNameFromQuery = queryParams.get('doctorName');

  // Ensure the default date is set to the current date
  const [formData, setFormData] = useState<AppointmentFormData>({
    patientName: '',
    doctorName: '',
    date: new Date().toISOString().split('T')[0], // Default to today
    time: new Date().toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    }), // Default to current time
    description: '',
    duration: 30, // Default duration
  });

  const { data: appointment } = useQuery({
    queryKey: ['appointment', id],
    queryFn: () => appointmentService.getById(Number(id)),
    enabled: isEditing,
  });

  const [suggestedTimes, setSuggestedTimes] = useState<string[]>([]);

  useEffect(() => {
    if (appointment) {
      setFormData({
        patientName: appointment.patientName,
        doctorName: appointment.doctorName,
        date: appointment.date,
        time: appointment.time,
        description: appointment.description || '',
        duration: appointment.duration || 30,
      });
    }
  }, [appointment]);

  useEffect(() => {
    if (doctorNameFromQuery) {
      setFormData((prev) => ({ ...prev, doctorName: doctorNameFromQuery }));
    }
  }, [doctorNameFromQuery]);

  const mutation = useMutation({
    mutationFn: (data: AppointmentFormData) =>
      isEditing
        ? appointmentService.update(Number(id), data).then(() => ({ success: true, suggestedTimes: [] }))
        : appointmentService.create(data),
    onSuccess: (data: CreateAppointmentResponse) => {
      if (data.success && (!data.suggestedTimes || data.suggestedTimes.length === 0)) {
        queryClient.invalidateQueries({ queryKey: ['appointments'] });
        if (isEditing) {
          navigate(`/appointments/${id}`); // Navigate to the appointment details page
        } else {
          navigate('/');
        }
      } else if (data.suggestedTimes && data.suggestedTimes.length > 0) {
        setSuggestedTimes(data.suggestedTimes);
        // Prevent redirection when conflicts are detected
        return;
      }
    },
    onError: (error: any) => {
      if (error.response?.data?.success === false && error.response?.data?.suggestedTimes?.length > 0) {
        setSuggestedTimes(error.response.data.suggestedTimes);
      } else {
        console.error('An unexpected error occurred:', error);
      }
      // Ensure the user stays on the current page
      setSuggestedTimes([]); // Clear suggested times if needed
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Ensure the time is sent to the backend as-is without formatting
    const adjustedFormData = {
      ...formData,
      time: formData.time, // Send time as-is
      duration: Number(formData.duration),
    };

    mutation.mutate(adjustedFormData);
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  return (
    <div className="max-w-2xl mx-auto">
      <button
        onClick={() => navigate('/')}
        className="fixed top-[50px] left-[50px] inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-gray-600 hover:bg-gray-700 shadow-lg"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={1.5}
          stroke="currentColor"
          className="w-5 h-5"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15.75 19.5L8.25 12l7.5-7.5"
          />
        </svg>
      </button>
      <h1 className="text-2xl font-semibold text-gray-800 mb-6">
        {isEditing ? 'Edit Appointment' : 'New Appointment'}
      </h1>
      <form onSubmit={handleSubmit} className="space-y-6 bg-gray-100 p-6 rounded-lg shadow">
        <div>
          <label
            htmlFor="patientName"
            className="block text-sm font-medium text-gray-800"
          >
            Patient Name
          </label>
          <input
            type="text"
            name="patientName"
            id="patientName"
            required
            value={formData.patientName}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
          />
        </div>

        <div>
          <label
            htmlFor="doctorName"
            className="block text-sm font-medium text-gray-800"
          >
            Doctor Name
          </label>
          <input
            type="text"
            name="doctorName"
            id="doctorName"
            required
            value={formData.doctorName}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
          />
        </div>

        <div>
          <label
            htmlFor="date"
            className="block text-sm font-medium text-gray-800"
          >
            Date
          </label>
          <input
            type="date"
            name="date"
            id="date"
            required
            value={formData.date}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
          />
        </div>

        <div>
          <label
            htmlFor="time"
            className="block text-sm font-medium text-gray-800"
          >
            Time
          </label>
          <input
            type="time"
            name="time"
            id="time"
            required
            value={formData.time}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
            step="300"
          />
        </div>

        <div>
          <label
            htmlFor="duration"
            className="block text-sm font-medium text-gray-800"
          >
            Duration (minutes)
          </label>
          <input
            type="number"
            name="duration"
            id="duration"
            required
            value={formData.duration}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
            min="1"
          />
        </div>

        <div>
          <label
            htmlFor="description"
            className="block text-sm font-medium text-gray-800"
          >
            Description
          </label>
          <textarea
            name="description"
            id="description"
            rows={3}
            value={formData.description}
            onChange={handleChange}
            className="mt-1 block w-full rounded-md border-gray-400 shadow-sm focus:border-blue-700 focus:ring-blue-700 bg-gray-50 text-gray-900"
          />
        </div>

        <div className="flex justify-end space-x-4">
          <button
            type="button"
            onClick={() => navigate('/')}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={mutation.isPending}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            {mutation.isPending ? 'Saving...' : isEditing ? 'Update' : 'Create'}
          </button>
        </div>
      </form>

      {suggestedTimes.length > 0 && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-md shadow-lg">
            <h2 className="text-lg font-semibold text-red-800">Date Conflict</h2>
            <p className="text-gray-800">The selected date and time are unavailable. Please consider the following available times:</p>
            <ul className="mt-2 list-disc list-inside">
              {suggestedTimes.map((time: any, index: number) => (
                <li key={index} className="text-gray-700">
                  {typeof time === 'string' ? time : `${new Date(time.appointmentStart).toLocaleDateString()} at ${new Date(time.appointmentStart).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`}
                </li>
              ))}
            </ul>
            <button
              onClick={() => setSuggestedTimes([])}
              className="mt-4 px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default AppointmentForm;