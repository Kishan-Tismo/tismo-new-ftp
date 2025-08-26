"use client";

import { useAuth } from "@/contexts/AuthContext";
import { Button } from "./ui/Button";
import Image from "next/image";
import { toast } from "sonner";

import logo from "@/assets/logo.png";
import logoutIcon from "@/assets/logout.png";

export default function Header() {
  const { logout } = useAuth();

  const handleLogout = () => {
    logout();
    toast.success("Logged out successfully");
  };

  return (
    <div className="header-div">
      <Image src={logo} alt="company-logo" width={125} title="Tismo" />
      <div className="company-code-container">
        <h1 className="text-lg font-bold" title="Client Code">
          tca207
        </h1>
        <div className="vertical-separator"></div>
        <h1 className="text-lg font-light" title="Client Name">
          Kern Laser Systems
        </h1>
      </div>
      <Image
        src={logoutIcon}
        alt="logout-button"
        width={35}
        title="Logout"
        onClick={handleLogout}
        style={{ cursor: "pointer" }}
      />
    </div>
  );
}
