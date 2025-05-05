import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import AppointmentForm from './components/AppointmentForm';
import AppointmentDetails from './components/AppointmentDetails';
import DoctorAppointments from './components/DoctorAppointments';
import headerImg from './assets/header.png';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <div className="min-h-screen w-screen bg-[#eaf6fb] flex flex-col items-center justify-start">
          <div className="w-full flex justify-center items-center">
            <div className="w-full flex justify-center">
              <img src={headerImg} alt="Medical Appointment Scheduling Application" className="w-full max-w-md mt-8 mb-8 rounded-lg shadow" style={{ objectFit: 'contain' }} />
            </div>
          </div>
          <main className="w-full flex-1 flex flex-col items-center justify-start">
            <div className="w-2/3 mx-auto">
              <Routes>
                <Route path="/" element={<DoctorAppointments />} />
                <Route path="/appointments/new" element={<AppointmentForm />} />
                <Route path="/appointments/:id" element={<AppointmentDetails />} />
                <Route path="/appointments/:id/edit" element={<AppointmentForm />} />
              </Routes>
            </div>
          </main>
        </div>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
