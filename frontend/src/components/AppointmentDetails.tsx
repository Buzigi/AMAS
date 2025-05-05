import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { appointmentService } from '../services/api';
import { useState, useEffect, ChangeEvent } from 'react';
import { AppointmentFormData } from '../types/appointment';

type AppointmentData = {
  suggestedTimes?: string[];
  // Add other properties of the data object here if needed
};

// Explicitly type suggestedTimes to include both strings and objects with an appointmentStart property
type SuggestedTime = string | { appointmentStart: string };

const AppointmentDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: appointment, isLoading, error } = useQuery({
    queryKey: ['appointment', id],
    queryFn: () => appointmentService.getById(Number(id)),
  });

  const deleteMutation = useMutation({
    mutationFn: () => appointmentService.delete(Number(id)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      navigate('/');
    },
  });

  const [suggestedTimes, setSuggestedTimes] = useState<SuggestedTime[]>([]);

  useEffect(() => {
    console.log('Suggested times state:', suggestedTimes);
    if (suggestedTimes.length > 0) {
      console.log('Pop-up should display with suggested times:', suggestedTimes);
    } else {
      console.log('No suggested times available to display.');
    }
  }, [suggestedTimes]);

  const updateMutation = useMutation({
    mutationFn: (updatedData: Partial<AppointmentFormData>) => appointmentService.update(Number(id), updatedData),
    onSuccess: (data) => {
      console.log('Update success response:', data);
      if (data.suggestedTimes && data.suggestedTimes.length > 0) {
        setSuggestedTimes(data.suggestedTimes);
      } else {
        setIsEditing(false);
        navigate('/');
      }
    },
    onError: (error: any) => {
      console.error('Update error response:', error);
      if (error.suggestedTimes && error.suggestedTimes.length > 0) {
        console.log('Setting suggested times from error response:', error.suggestedTimes);
        setSuggestedTimes(error.suggestedTimes);
      } else {
        console.error('An unexpected error occurred:', error);
      }
    },
  });

  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState<AppointmentFormData>({
    patientName: '',
    doctorName: '',
    date: '',
    time: '',
    description: '',
    duration: 0,
  });

  useEffect(() => {
    if (appointment) {
      setFormData({
        patientName: appointment.patientName,
        doctorName: appointment.doctorName,
        date: appointment.date,
        time: appointment.time,
        description: appointment.description || '',
        duration: appointment.duration || 0,
      });
    }
  }, [appointment]);

  useEffect(() => {
    console.log('Appointment data:', appointment);
  }, [appointment]);

  useEffect(() => {
    console.log('Error response:', error);
  }, [error]);

  const handleInputChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSave = () => {
    console.log('Saving appointment with data:', formData);
    updateMutation.mutate(formData, {
      onSuccess: (data: AppointmentData) => {
        console.log('Update successful:', data);
        setIsEditing(false); // Ensure editing mode is exited
        if (!('suggestedTimes' in data) || (data.suggestedTimes ?? []).length === 0) {
          navigate('/'); // Navigate back to the main page only if no conflicts
        }
      },
      onError: (error: any) => {
        console.error('Update failed:', error);
        if (error.suggestedTimes && error.suggestedTimes.length > 0) {
          console.log('Setting suggested times from error response:', error.suggestedTimes);
          setSuggestedTimes(error.suggestedTimes);
        } else {
          console.error('An unexpected error occurred:', error);
          alert('An unexpected error occurred. Please try again later.');
        }
      },
    });
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error || !appointment) {
    return (
      <div className="text-center text-red-600">
        Error loading appointment details. Please try again later.
      </div>
    );
  }

  return (
    <div style={{ marginBottom: '20px' }}>
      <div className="relative max-w-2xl mx-auto">
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
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6 flex justify-between items-center">
            <div>
              <h3 className="text-lg leading-6 font-medium text-gray-900">
                Appointment Details
              </h3>
              <p className="mt-1 max-w-2xl text-sm text-gray-500">
                Patient and appointment information
              </p>
            </div>
            <div className="flex space-x-4">
              {isEditing ? (
                <button
                  onClick={handleSave}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700"
                >
                  Save
                </button>
              ) : (
                <button
                  onClick={() => setIsEditing(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                >
                  Update
                </button>
              )}
              <button
                onClick={() => deleteMutation.mutate()}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700"
              >
                Delete
              </button>
            </div>
          </div>
          <div className="border-t border-gray-200">
            <dl>
              {isEditing ? (
                <>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Patient Name</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <input
                        type="text"
                        name="patientName"
                        value={formData.patientName}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                  <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Doctor Name</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <input
                        type="text"
                        name="doctorName"
                        value={formData.doctorName}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Date</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <input
                        type="date"
                        name="date"
                        value={formData.date}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                  <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Time</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <input
                        type="time"
                        name="time"
                        value={formData.time}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Duration</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <input
                        type="number"
                        name="duration"
                        value={formData.duration}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Description</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      <textarea
                        name="description"
                        value={formData.description}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-md p-2"
                      />
                    </dd>
                  </div>
                </>
              ) : (
                <>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Patient Name</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {appointment.patientName}
                    </dd>
                  </div>
                  <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Doctor Name</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {appointment.doctorName}
                    </dd>
                  </div>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Date</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {new Date(appointment.date).toLocaleDateString()}
                    </dd>
                  </div>
                  <div className="bg-white px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Time</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {appointment.time}
                    </dd>
                  </div>
                  <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                    <dt className="text-sm font-medium text-gray-500">Duration</dt>
                    <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                      {appointment.duration} minutes
                    </dd>
                  </div>
                  {appointment.description && (
                    <div className="bg-gray-50 px-4 py-5 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-6">
                      <dt className="text-sm font-medium text-gray-500">Description</dt>
                      <dd className="mt-1 text-sm text-gray-900 sm:mt-0 sm:col-span-2">
                        {appointment.description}
                      </dd>
                    </div>
                  )}
                </>
              )}
            </dl>
          </div>
        </div>
      </div>
      {suggestedTimes.length > 0 && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-md shadow-lg">
            <h2 className="text-lg font-semibold text-red-800">Date Conflict</h2>
            <p className="text-gray-800">The selected date and time are unavailable. Please consider the following available times:</p>
            <ul className="mt-2 list-disc list-inside">
              {suggestedTimes.map((time, index) => (
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

export default AppointmentDetails;