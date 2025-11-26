import { BrowserRouter, Routes, Route, Link, useLocation } from 'react-router-dom';
import { EstimateForm } from './components/EstimateForm';
import { JobsList } from './components/JobsList';
import { FileText, Briefcase } from 'lucide-react';

function Navigation() {
  const location = useLocation();
  
  const isActive = (path: string) => {
    return location.pathname === path;
  };
  
  return (
    <nav className="bg-white border-b">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          <div className="flex items-center space-x-8">
            <h1 className="text-xl font-bold text-green-600">VerdaVida Lawn Care</h1>
            <div className="flex space-x-1">
              <Link
                to="/"
                className={`flex items-center gap-2 px-4 py-2 rounded-md transition-colors ${
                  isActive('/')
                    ? 'bg-green-50 text-green-700 font-medium'
                    : 'text-gray-600 hover:bg-gray-50'
                }`}
              >
                <FileText className="h-4 w-4" />
                Create Estimate
              </Link>
              <Link
                to="/jobs"
                className={`flex items-center gap-2 px-4 py-2 rounded-md transition-colors ${
                  isActive('/jobs')
                    ? 'bg-green-50 text-green-700 font-medium'
                    : 'text-gray-600 hover:bg-gray-50'
                }`}
              >
                <Briefcase className="h-4 w-4" />
                Jobs
              </Link>
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
}

function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50">
        <Navigation />
        <Routes>
          <Route path="/" element={<EstimateForm />} />
          <Route path="/jobs" element={<JobsList />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App