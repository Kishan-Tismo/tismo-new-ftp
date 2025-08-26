"use client";

import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";

interface AuthContextType {
  token: string | null;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem("token");
      if (storedToken) {
        setToken(storedToken);
      }
    } catch (e) {
      // localStorage may be unavailable (Safari private mode, etc.)
      console.warn("localStorage unavailable", e);
    }
  }, []);

  const login = (newToken: string) => {
    setToken(newToken);
    try {
      localStorage.setItem("token", newToken);
    } catch (e) {
      // localStorage may be unavailable
      console.warn("localStorage unavailable", e);
    }
  };

  const logout = () => {
    setToken(null);
    try {
      localStorage.removeItem("token");
    } catch (e) {
      // localStorage may be unavailable
      console.warn("localStorage unavailable", e);
    }
  };

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
