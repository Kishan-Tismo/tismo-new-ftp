"use client";

import { useAuth } from "@/contexts/AuthContext";

export default function Footer() {
  const { token } = useAuth();

  return (
    // Patch notes to be added here
    <footer className="login-footer">
      <div>© 2025 Tismo Technology Solutions. All Rights Reserved.</div>

      {/* Show only if logged in */}
      {token && <h2 className="title-footer">TRANSFER</h2>}
    </footer>
  );
}
