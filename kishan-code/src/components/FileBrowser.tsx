"use client";

import { useState, useEffect } from "react";
import { PathNode } from "@/lib/types";
import FileItem from "./FileItem";
import Header from "./Header";
import { Button } from "./ui/Button";
import { Input } from "./ui/Input";
import * as api from "@/services/api";
import { useAuth } from "@/contexts/AuthContext";
import { sanitizeFileName } from "@/lib/utils";
import DirectorySelector from "./DirectorySelector";
import PathBar from "./PathBar";
import ViewToggle from "./ViewToggle";
import FileActions from "./FileActions";
import FolderActions from "./FolderActions";
import FileTable from "./FileTable";
import Footer from "./Footer";

export default function FileBrowser() {
  const [pathNodes, setPathNodes] = useState<PathNode[]>([]);
  const [currentFolder, setCurrentFolder] = useState("/");
  const [newFolderName, setNewFolderName] = useState("");
  const [showNewFolderInput, setShowNewFolderInput] = useState(false);
  const [message, setMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const { token, logout } = useAuth();

  const fetchFiles = async () => {
    if (!token) return;
    setMessage("Fetching files...");
    try {
      const files = await api.getFileList(currentFolder, token);
      setPathNodes(files);
      setMessage("");
    } catch (err) {
      console.error(err);
      setErrorMessage("Failed to fetch files.");
      if (err instanceof Error && err.message.includes("401")) {
        logout();
      }
    }
  };

  useEffect(() => {
    fetchFiles();
  }, [currentFolder, token]);

  const handleSelect = (item: PathNode) => {
    if (item.isFolder) {
      const newPath = `${currentFolder}${item.name}/`.replace(/\/\//g, "/");
      setCurrentFolder(newPath);
    }
  };

  const handleDelete = async (item: PathNode) => {
    if (!token) return;
    const confirmation = window.confirm(
      `Are you sure you want to delete ${item.isFolder ? "folder" : "file"} "${
        item.name
      }"?`
    );
    if (!confirmation) return;

    setMessage(`Deleting ${item.name}...`);
    try {
      const path = `${currentFolder}${item.name}`;
      if (item.isFolder) {
        await api.deleteFolder(path, token);
      } else {
        await api.deleteFile(path, token);
      }
      setMessage(`Deleted ${item.name} successfully.`);
      fetchFiles();
    } catch (err) {
      console.error(err);
      setErrorMessage(`Failed to delete ${item.name}.`);
    }
  };

  const handleDownload = async (item: PathNode) => {
    if (!token) return;
    setMessage(`Downloading ${item.name}...`);
    try {
      const path = `${currentFolder}${item.name}`;
      await api.downloadFile(path, token);
      setMessage(`Downloaded ${item.name} successfully.`);
    } catch (err) {
      console.error(err);
      setErrorMessage(`Failed to download ${item.name}.`);
    }
  };

  const handleCreateFolder = async () => {
    if (!token || !newFolderName) return;
    const sanitizedName = sanitizeFileName(newFolderName);
    if (sanitizedName !== newFolderName) {
      setErrorMessage("Invalid folder name.");
      return;
    }

    setMessage(`Creating folder ${sanitizedName}...`);
    try {
      const path = `${currentFolder}${sanitizedName}`;
      await api.createFolder(path, token);
      setNewFolderName("");
      setShowNewFolderInput(false);
      setMessage(`Created folder ${sanitizedName} successfully.`);
      fetchFiles();
    } catch (err) {
      console.error(err);
      setErrorMessage(`Failed to create folder ${sanitizedName}.`);
    }
  };

  const handleFileUpload = async (files: FileList | null) => {
    if (!token || !files || files.length === 0) return;

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const sanitizedName = sanitizeFileName(file.name);
      if (sanitizedName !== file.name) {
        setErrorMessage(`Invalid character in file name: ${file.name}`);
        return;
      }
    }

    setMessage("Uploading files...");
    try {
      await api.uploadFiles(currentFolder, files, token, (progress) => {
        setMessage(`Uploading... ${Math.round(progress)}%`);
      });
      setMessage("Files uploaded successfully.");
      fetchFiles();
    } catch (err) {
      console.error(err);
      setErrorMessage("File upload failed.");
    }
  };

  const goUp = () => {
    if (currentFolder === "/") return;
    const parent = currentFolder.split("/").slice(0, -2).join("/") + "/";
    setCurrentFolder(parent || "/");
  };

  return (
    <div className="file-browser-container">
      <Header />
      <DirectorySelector />
      <PathBar />
      <div className="tables-container">
        <div className="action-container">
          <FileActions />
          <br />
          <FolderActions />
        </div>
        <div className="documents-container">
          <FileTable />
        </div>
      </div>
    </div>
  );
}
