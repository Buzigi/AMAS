import { Link } from 'react-router-dom';

const Navbar = () => {
  return (
    <nav className="bg-[#eaf6fb] shadow h-screen w-56 flex flex-col items-center py-10">
      <Link to="/" className="text-2xl font-bold text-gray-800 mb-10">
        MedSched
      </Link>
      <Link
        to="/appointments/new"
        className="mb-4 px-4 py-2 rounded text-gray-700 bg-white hover:bg-blue-100 shadow text-base font-medium w-full text-center"
      >
        New Appointment
      </Link>
    </nav>
  );
};

export default Navbar; 