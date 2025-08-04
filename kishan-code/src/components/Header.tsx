"use client"

import { useAuth } from "@/contexts/AuthContext";
import { Button } from "./ui/Button";

export default function Header() {
  const { logout } = useAuth();

  const handleLogout = () => {
    logout();
  };

  return (
    <header className="flex justify-between items-center p-4 bg-gray-100 dark:bg-gray-800">
      <h1 className="text-xl font-bold">File Transfer</h1>
      <Button onClick={handleLogout}>Logout</Button>
    </header>
  );
}
