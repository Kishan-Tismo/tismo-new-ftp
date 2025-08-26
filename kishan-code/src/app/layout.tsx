import type { Metadata } from "next";
import { Montserrat } from "next/font/google";
import "./globals.css";
import { AuthProvider } from "@/contexts/AuthContext";
import Footer from "@/components/Footer";
import { FolderProvider } from "@/contexts/FolderContext";
import { Toaster } from "@/components/ui/sonner";

const montserrat = Montserrat({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"], // choose as needed
  variable: "--font-montserrat",
});

export const metadata: Metadata = {
  title: "File Transfer",
  description: "File Transfer Application",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={`${montserrat.variable} antialiased`}>
        <AuthProvider>
          <FolderProvider>
            <Toaster position="top-center" richColors duration={1000} />
            {children}
            <Footer />
          </FolderProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
