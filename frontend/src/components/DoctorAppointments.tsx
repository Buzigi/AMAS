import { useQuery } from '@tanstack/react-query';
import { appointmentService } from '../services/api';
import { Appointment } from '../types/appointment';
import { Link } from 'react-router-dom';
import { useState, useEffect } from 'react';

const DOCTOR_HEADER_HEIGHT = '48px'; // 3rem

// Ensure proper date formatting in DoctorAppointments
const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? 'Invalid Date' : date.toLocaleDateString();
};

const DoctorAppointments = () => {
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

  const handleSearch = (term: string) => {
    setSearchTerm(term);
    if (term.trim() === '') {
      setFilteredAppointments(appointments || []);
      return;
    }
    const filtered = appointments?.filter(
      (appointment) =>
        appointment.doctorName.toLowerCase().includes(term.toLowerCase()) ||
        appointment.patientName.toLowerCase().includes(term.toLowerCase())
    );
    setFilteredAppointments(filtered || []);
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

  const appointmentsByDoctor = filteredAppointments.reduce((acc, appointment) => {
    const doctor = appointment.doctorName;
    if (!acc[doctor]) {
      acc[doctor] = [];
    }
    acc[doctor].push(appointment);
    return acc;
  }, {} as Record<string, Appointment[]>);

  return (
    // Added margin-bottom to the main container
    <div className="w-full px-8" style={{ marginBottom: '20px' }}>
      <h1 className="text-2xl font-bold text-gray-900 mb-6 text-center">Appointments by Doctor</h1>
      <input
        type="text"
        value={searchTerm}
        onChange={(e) => handleSearch(e.target.value)}
        placeholder="Search by doctor or patient name"
        className="mb-4 p-2 border border-gray-300 rounded w-full"
      />
      <div className="flex flex-col gap-8 w-full">
        {Object.entries(appointmentsByDoctor).map(([doctor, doctorAppointments]) => (
          <div key={doctor} className="bg-white rounded-lg shadow p-6 w-full">
            <div
              className="flex items-center gap-2 mb-4"
              style={{ minHeight: DOCTOR_HEADER_HEIGHT, height: DOCTOR_HEADER_HEIGHT }}
            >
              <h2 className="text-lg font-semibold text-gray-900 flex-1 truncate">{doctor}</h2>
              <Link
                to={`/appointments/new?doctorName=${encodeURIComponent(doctor)}`}
                className="ml-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                title={`Add appointment for ${doctor}`}
              >
                New Appointment
              </Link>
            </div>
            <div className="flex flex-row gap-4 overflow-x-auto pb-2">
              {doctorAppointments.map((appointment) => {
                const appointmentDate = new Date(appointment.date + 'T' + appointment.time);
                const currentDate = new Date();
                const appointmentEndDate = new Date(appointmentDate.getTime() + (appointment.duration || 0) * 60000);

                let status = '';
                let statusColor = '';

                if (currentDate > appointmentEndDate) {
                  status = 'Done';
                  statusColor = 'text-red-500';
                } else if (currentDate >= appointmentDate && currentDate <= appointmentEndDate) {
                  status = 'In Progress';
                  statusColor = 'text-blue-500';
                } else {
                  status = 'Scheduled';
                  statusColor = 'text-green-500';
                }

                return (
                  <Link
                    to={`/appointments/${appointment.id}`}
                    key={appointment.id}
                    className="min-w-[220px] max-w-xs p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors flex-shrink-0"
                  >
                    <div className="flex flex-col">
                      <p className="font-medium text-gray-900">{appointment.patientName}</p>
                      <p className="text-sm text-gray-500">
                        {appointment.date} at {appointment.time}
                      </p>
                      <p className="text-sm text-gray-500">Duration: {appointment.duration} minutes</p>
                      <p className={`text-sm font-semibold ${statusColor}`}>{status}</p>
                      {appointment.description && (
                        <p className="mt-2 text-sm text-gray-600">{appointment.description}</p>
                      )}
                    </div>
                  </Link>
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DoctorAppointments;