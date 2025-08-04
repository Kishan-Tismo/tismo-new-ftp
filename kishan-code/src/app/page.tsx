"use client"

import LoginPage from "@/components/LoginPage";
import FileBrowser from "@/components/FileBrowser";
import { useAuth } from "@/contexts/AuthContext";

export default function Home() {
  const { token } = useAuth();

  return (
    <main>
      {token ? <FileBrowser /> : <LoginPage />}
    </main>
  );
}
