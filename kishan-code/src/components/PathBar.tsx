import Image from "next/image";
import backButtonIcon from "@/assets/back_icon.png";
import copyIcon from "@/assets/copy_icon.png";
import { useFolder } from "@/contexts/FolderContext";
import { toast } from "sonner";

export default function PathBar() {
  const { currentFolder, setCurrentFolder } = useFolder();

  const handleBack = () => {
    if (currentFolder === "/") return;
    const parent = currentFolder.split("/").slice(0, -2).join("/") + "/";
    setCurrentFolder(parent || "/");
  };

  const handleCopy = async () => {
    // Clipboard API fallback for Safari and unsupported browsers
    try {
      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(currentFolder);
      } else {
        // fallback for Safari/private/older browsers
        const input = document.createElement("input");
        input.value = currentFolder;
        document.body.appendChild(input);
        input.select();
        document.execCommand("copy");
        document.body.removeChild(input);
      }
      toast.info("Path copied to clipboard");
    } catch {
      toast.error("Failed to copy path");
    }
  };

  return (
    <div className="path-container">
      <button
        className="back-button"
        title="Go to previous folder"
        onClick={handleBack}
      >
        <Image src={backButtonIcon} width={30} alt="back-button" />
      </button>
      <div className="path-description">
        <span className="black-text">Path:</span>
        <input
          className="path-input"
          id="pathInput"
          value={currentFolder}
          readOnly
        />
      </div>
      <div
        className="info-icon"
        id="copyToClipboard"
        title="Copy path to clipboard"
        onClick={handleCopy}
        style={{ cursor: "pointer" }}
      >
        <Image src={copyIcon} alt="copy-icon" width={30} />
      </div>
    </div>
  );
}
