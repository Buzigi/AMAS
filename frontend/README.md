# MedSched Frontend

A modern React frontend for the MedSched appointment scheduling system.

## Features

- View all appointments in a clean, organized list
- Create new appointments
- Edit existing appointments
- Delete appointments
- View detailed appointment information
- Responsive design that works on all devices

## Tech Stack

- React 18
- TypeScript
- React Query for data fetching
- React Router for navigation
- Tailwind CSS for styling
- Axios for API requests

## Getting Started

1. Install dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm run dev
   ```

3. Build for production:
   ```bash
   npm run build
   ```

## Development

The frontend is configured to connect to the MedSched API running on `http://localhost:5077`. Make sure the API is running before starting the frontend application.

## Project Structure

- `src/components/` - React components
- `src/services/` - API service functions
- `src/types/` - TypeScript type definitions
- `src/App.tsx` - Main application component
- `src/main.tsx` - Application entry point
