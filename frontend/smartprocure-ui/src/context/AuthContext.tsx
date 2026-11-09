import React, { createContext, useContext, useState, useEffect } from 'react';
import type { User, UserRole } from '../types';
import { authApi } from '../services/api';

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  quickLogin: (role: UserRole) => Promise<boolean>;
  logout: () => void;
  hasRole: (roles: UserRole[]) => boolean;
  isAdmin: boolean;
  isPurchasing: boolean;
  isWarehouse: boolean;
  isFinance: boolean;
  isRequester: boolean;
  isManagement: boolean;
}

const DEMO_CREDENTIALS: Record<UserRole, { email: string; pass: string }> = {
  Administrator: { email: 'admin@smartprocure.com', pass: 'Admin@123' },
  RequisitionCreator: { email: 'requester@smartprocure.com', pass: 'Request@123' },
  PurchaseManager: { email: 'purchasemanager@smartprocure.com', pass: 'Purchase@123' },
  WarehouseManager: { email: 'warehouse@smartprocure.com', pass: 'Warehouse@123' },
  FinanceManager: { email: 'finance@smartprocure.com', pass: 'Finance@123' },
  ManagementViewer: { email: 'management@smartprocure.com', pass: 'Manage@123' },
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('smartprocure_user');
    return saved ? JSON.parse(saved) : null;
  });
  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem('smartprocure_token');
  });
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const verifyUser = async () => {
      if (token) {
        try {
          const res = await authApi.me();
          if (res.data.success) {
            setUser(res.data.data);
            localStorage.setItem('smartprocure_user', JSON.stringify(res.data.data));
          }
        } catch {
          // If token invalid, clear
          logout();
        }
      }
      setLoading(false);
    };

    verifyUser();
  }, [token]);

  const login = async (email: string, pass: string): Promise<boolean> => {
    try {
      const res = await authApi.login({ email, password: pass });
      if (res.data.success && res.data.data) {
        const { token: jwt, user: userData } = res.data.data;
        setToken(jwt);
        setUser(userData);
        localStorage.setItem('smartprocure_token', jwt);
        localStorage.setItem('smartprocure_user', JSON.stringify(userData));
        return true;
      }
      return false;
    } catch {
      return false;
    }
  };

  const quickLogin = async (role: UserRole): Promise<boolean> => {
    const creds = DEMO_CREDENTIALS[role];
    if (!creds) return false;
    return await login(creds.email, creds.pass);
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('smartprocure_token');
    localStorage.removeItem('smartprocure_user');
  };

  const hasRole = (roles: UserRole[]): boolean => {
    if (!user) return false;
    return roles.includes(user.roleName);
  };

  const isAdmin = user?.roleName === 'Administrator';
  const isPurchasing = user?.roleName === 'PurchaseManager' || isAdmin;
  const isWarehouse = user?.roleName === 'WarehouseManager' || isAdmin;
  const isFinance = user?.roleName === 'FinanceManager' || isAdmin;
  const isRequester = user?.roleName === 'RequisitionCreator' || isAdmin;
  const isManagement = user?.roleName === 'ManagementViewer' || isAdmin;

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        login,
        quickLogin,
        logout,
        hasRole,
        isAdmin,
        isPurchasing,
        isWarehouse,
        isFinance,
        isRequester,
        isManagement,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

