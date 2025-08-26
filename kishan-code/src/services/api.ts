import { PathNode } from "@/lib/types";

const API_BASE_URL = getBaseUrl(); // Assuming the backend runs on port 5000

export function getBaseUrl() {
  if (typeof window !== "undefined") {
    // Client-side
    return window.location.origin;
  } else {
    // Server-side (Next.js SSR)
    return process.env.NEXT_PUBLIC_BASE_URL || "http://localhost:5000";
  }
}

export async function login(
  username: string,
  password: string
): Promise<string> {
  const response = await fetch(
    `${API_BASE_URL}/login?user=${username}&pwd=${password}`
  );
  if (!response.ok) {
    throw new Error("Login failed");
  }
  return response.json();
}

export async function getFileList(
  folder: string,
  token: string
): Promise<PathNode[]> {
  console.info(token);

  const response = await fetch(
    `${API_BASE_URL}/fileList?folderName=${encodeURI(folder)}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
    }
  );
  if (!response.ok) {
    throw new Error("Failed to fetch file list");
  }
  const data = await response.json();
  return data.map((pnode: any) => ({
    ...pnode,
    createdOn: new Date(pnode.createdOn),
  }));
}

export async function createFolder(
  folderName: string,
  token: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/createFolder?folderName=${encodeURI(folderName)}`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  if (!response.ok) {
    throw new Error("Failed to create folder");
  }
}

export async function deleteFile(
  fileName: string,
  token: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/deleteFile/${encodeURIComponent(fileName)}`,
    {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  if (!response.ok) {
    throw new Error("Failed to delete file");
  }
}

export async function deleteFolder(
  folderName: string,
  token: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/deleteFolder/${encodeURIComponent(folderName)}`,
    {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  if (!response.ok) {
    throw new Error("Failed to delete folder");
  }
}

export async function uploadFiles(
  folder: string,
  files: FileList,
  token: string,
  onUploadProgress: (progress: number) => void
): Promise<void> {
  const formData = new FormData();
  for (let i = 0; i < files.length; i++) {
    formData.append("file", files[i], files[i].name);
  }

  const xhr = new XMLHttpRequest();
  xhr.open(
    "POST",
    `${API_BASE_URL}/upload?folderName=${encodeURI(folder)}`,
    true
  );
  xhr.setRequestHeader("Authorization", `Bearer ${token}`);

  xhr.upload.onprogress = (event) => {
    if (event.lengthComputable) {
      const percentComplete = (event.loaded / event.total) * 100;
      onUploadProgress(percentComplete);
    }
  };

  return new Promise((resolve, reject) => {
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve();
      } else {
        reject(new Error(xhr.statusText));
      }
    };
    xhr.onerror = () => {
      reject(new Error("Upload failed"));
    };
    xhr.send(formData);
  });
}

export async function downloadFile(
  fileName: string,
  token: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/download?filename=${encodeURI(fileName)}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (!response.ok) {
    throw new Error("Download failed");
  }

  const blob = await response.blob();
  const contentDisposition = response.headers.get("content-disposition");
  let filename = fileName.split("/").pop() || "downloaded-file";

  if (contentDisposition) {
    const match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/);
    if (match && match.length > 1) {
      filename = decodeURIComponent(match[1]);
    } else {
      const filenameMatch = contentDisposition.match(/filename="([^;]+)/);
      if (filenameMatch && filenameMatch.length > 1) {
        filename = filenameMatch[1];
      }
    }
  }

  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}
