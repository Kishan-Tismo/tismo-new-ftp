"use client";

import { createContext, useContext, useState, ReactNode } from "react";

interface FolderContextType {
  currentFolder: string;
  setCurrentFolder: (folder: string) => void;
}

const FolderContext = createContext<FolderContextType | undefined>(undefined);

export function FolderProvider({ children }: { children: ReactNode }) {
  const [currentFolder, setCurrentFolder] = useState<string>("/");
  return (
    <FolderContext.Provider value={{ currentFolder, setCurrentFolder }}>
      {children}
    </FolderContext.Provider>
  );
}

export function useFolder() {
  const context = useContext(FolderContext);
  if (!context) {
    throw new Error("useFolder must be used within a FolderProvider");
  }
  return context;
}
