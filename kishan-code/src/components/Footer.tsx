"use client";

import { useAuth } from "@/contexts/AuthContext";

export default function Footer() {
  const { token } = useAuth();

  return (
    // Patch notes to be added here
    <footer className="login-footer">
      {<h2 className="version-footer">v1.0</h2>}
      <div>© 2025 Tismo Technology Solutions. All Rights Reserved.</div>

      {/* Show only if logged in */}
      {token && <h2 className="title-footer">TRANSFER</h2>}
    </footer>
  );
}
