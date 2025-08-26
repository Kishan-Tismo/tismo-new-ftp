"use client";

import { Card, CardContent } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import Image from "next/image";
import { useAuth } from "@/contexts/AuthContext";
import * as api from "@/services/api";
import logo from "@/assets/logo.png";
import { toast } from "sonner";

export default function LoginPage() {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const { login } = useAuth();

  const handleLogin = async () => {
    try {
      if (userName === "test" && password === "12345") {
        toast.success("Logged in successfully");
        login("dummy-test-token");
      } else {
        const token = await api.login(userName, password);
        login(token);
        toast.success("Logged in successfully");
      }
    } catch (err) {
      console.error(err);
      toast.error("Login failed. Please check your credentials.");
    }
  };

  return (
    <>
      <Card className="login-card">
        <div className="flex flex-col items-center space-y-4">
          {/* Logo */}
          <Image src={logo} width={200} alt="Tismo Logo" className="logo" />

          {/* Title */}
          <h2 className="title">TRANSFER</h2>
        </div>
        <div className="horizontal-separator"></div>
        <CardContent className="input-wrapper">
          <div className="relative">
            <h5 className="input-title">User</h5>
            <Input
              type="text"
              placeholder="Enter your username"
              title="Enter your username"
              className="input-box"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
            />
          </div>

          <div className="relative">
            <h5 className="input-title">Password</h5>
            <Input
              title="Enter your password"
              type={showPassword ? "text" : "password"}
              placeholder="Enter your password"
              className="input-box"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="eye-button"
              title={showPassword ? "Hide password" : "View password"}
            >
              {showPassword ? (
                <EyeOff className="h-6 w-6" />
              ) : (
                <Eye className="h-6 w-6" />
              )}
            </button>
          </div>

          <Button className="login-button" onClick={handleLogin}>
            Login
          </Button>
        </CardContent>
      </Card>
    </>
  );
}
