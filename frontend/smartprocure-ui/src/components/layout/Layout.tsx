import React from 'react';
import { Outlet, useLocation, Link } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { ChevronRight, Home } from 'lucide-react';

export const Layout: React.FC = () => {
  const location = useLocation();

  // Generate breadcrumb items
  const pathnames = location.pathname.split('/').filter((x) => x);

  const formatBreadcrumb = (name: string) => {
    return name
      .replace(/-/g, ' ')
      .replace(/\b\w/g, (l) => l.toUpperCase());
  };

  return (
    <div className="flex h-screen bg-slate-50 overflow-hidden font-sans">
      {/* Sidebar */}
      <Sidebar />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Header />

        {/* Breadcrumb Navigation Bar */}
        <div className="bg-white border-b border-slate-200/80 px-6 py-2 flex items-center gap-2 text-xs text-slate-500">
          <Link to="/" className="flex items-center gap-1 hover:text-blue-600 transition-colors">
            <Home className="w-3.5 h-3.5" />
            <span>Home</span>
          </Link>
          {pathnames.map((name, index) => {
            const routeTo = `/${pathnames.slice(0, index + 1).join('/')}`;
            const isLast = index === pathnames.length - 1;

            return (
              <React.Fragment key={name}>
                <ChevronRight className="w-3 h-3 text-slate-400" />
                {isLast ? (
                  <span className="font-semibold text-slate-800">{formatBreadcrumb(name)}</span>
                ) : (
                  <Link to={routeTo} className="hover:text-blue-600 transition-colors">
                    {formatBreadcrumb(name)}
                  </Link>
                )}
              </React.Fragment>
            );
          })}
        </div>

        {/* Page Body Viewport */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
