import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { appointmentService } from '../services/api';
import { Appointment } from '../types/appointment';
import { useState, useEffect } from 'react';

const AppointmentList = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filteredAppointments, setFilteredAppointments] = useState<Appointment[]>([]);

  const { data: appointments, isLoading, error } = useQuery<Appointment[]>({
    queryKey: ['appointments'],
    queryFn: appointmentService.getAll,
  });

  useEffect(() => {
    if (appointments) {
      setFilteredAppointments(appointments);
    }
  }, [appointments]);

  const handleSearch = async (term: string) => {
    setSearchTerm(term);
    if (term.trim() === '') {
      setFilteredAppointments(appointments || []);
      return;
    }
    const suggestions = await appointmentService.search(term);
    setFilteredAppointments(suggestions);
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center text-red-600">
        Error loading appointments. Please try again later.
      </div>
    );
  }

  return (
    <div style={{ marginBottom: '20px' }}>
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Appointments</h1>
      <input
        type="text"
        value={searchTerm}
        onChange={(e) => handleSearch(e.target.value)}
        placeholder="Search by doctor or patient name"
        className="mb-4 p-2 border border-gray-300 rounded w-full"
      />
      <div className="bg-white shadow overflow-hidden sm:rounded-md">
        <ul className="divide-y divide-gray-200">
          {filteredAppointments.map((appointment) => (
            <li key={appointment.id}>
              <Link
                to={`/appointments/${appointment.id}`}
                className="block hover:bg-gray-50"
              >
                <div className="px-4 py-4 sm:px-6">
                  <div className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-blue-600 truncate">
                        {appointment.patientName}{' '}
                        {appointment.duration && (
                          <span className="font-bold">({appointment.duration} mins)</span>
                        )}
                      </p>
                      <p className="mt-1 text-sm text-gray-500">
                        Doctor: {appointment.doctorName}
                      </p>
                    </div>
                    <div className="ml-4 flex-shrink-0">
                      <div className="flex flex-col items-end">
                        <p className="text-sm text-gray-900">
                          {new Date(appointment.date).toLocaleDateString()}
                        </p>
                        <p className="text-sm text-gray-500">{appointment.time}</p>
                      </div>
                    </div>
                  </div>
                  <div className="mt-2">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        appointment.status === 'scheduled'
                          ? 'bg-green-100 text-green-800'
                          : appointment.status === 'completed'
                          ? 'bg-blue-100 text-blue-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {appointment.status.charAt(0).toUpperCase() +
                        appointment.status.slice(1)}
                    </span>
                  </div>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};

export default AppointmentList;