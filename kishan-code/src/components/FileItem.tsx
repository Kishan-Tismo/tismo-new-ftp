"use client"

import { PathNode } from "@/lib/types";
import { Button } from "./ui/Button";

interface FileItemProps {
  item: PathNode;
  onSelect: (item: PathNode) => void;
  onDelete: (item: PathNode) => void;
  onDownload: (item: PathNode) => void;
}

export default function FileItem({ item, onSelect, onDelete, onDownload }: FileItemProps) {
  const isFolder = item.isFolder;

  const fileSizeStr = (number: number) => {
    const oneKb = 1024;
    const oneMb = oneKb * oneKb;
    const oneGb = oneMb * oneKb;
    const oneTb = oneGb * oneKb;

    if (number < oneKb) {
      return number + " bytes";
    }
    if (number >= oneKb && number < oneMb) {
      return (number / oneKb).toFixed(1) + " KB";
    }
    if (number >= oneMb && number < oneGb) {
      return (number / oneMb).toFixed(2) + " MB";
    }
    if (number >= oneGb && number < oneTb) {
      return (number / oneGb).toFixed(2) + " GB";
    }
    if (number >= oneTb) {
      return (number / oneTb).toFixed(2) + " TB";
    }
    return "";
  };

  return (
    <div className="flex items-center justify-between p-2 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-md">
      <div className="flex items-center gap-2 cursor-pointer" onClick={() => onSelect(item)}>
        <span>{isFolder ? "📁" : "📄"}</span>
        <span>{item.name}</span>
      </div>
      <div className="flex items-center gap-2">
        {!isFolder && <span>{fileSizeStr(item.size)}</span>}
        <span>{new Date(item.createdOn).toLocaleDateString()}</span>
        {!isFolder && (
          <Button variant="outline" size="sm" onClick={() => onDownload(item)}>
            Download
          </Button>
        )}
        <Button variant="destructive" size="sm" onClick={() => onDelete(item)}>
          Delete
        </Button>
      </div>
    </div>
  );
}
