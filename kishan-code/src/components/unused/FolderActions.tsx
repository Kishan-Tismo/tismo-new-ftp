import uploadFolderIcon from "@/assets/upload_folder.png";
import uploadFolderActiveIcon from "@/assets/upload_folder_active.png";
import downloadFolderIcon from "@/assets/download_folder.png";
import downloadFolderActiveIcon from "@/assets/download_folder_active.png";
import renameIcon from "@/assets/edit.png";
import renameActiveIcon from "@/assets/edit_active.png";
import deleteIcon from "@/assets/delete.png";
import deleteActiveIcon from "@/assets/delete_active.png";
import Image from "next/image";

export default function FolderActions() {
  return (
    <div className="action-panel">
      <h3 className="black-text">Folder Actions</h3>

      {/* Upload Folder */}
      <button className="action-button-radio">
        <Image
          src={uploadFolderIcon}
          alt="upload-folder"
          width={25}
          className="icon default"
        />
        <Image
          src={uploadFolderActiveIcon}
          alt="upload-folder-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Upload or Drop</span>
      </button>

      {/* Download Folder */}
      <button className="action-button-radio">
        <Image
          src={downloadFolderIcon}
          alt="download-folder"
          width={25}
          className="icon default"
        />
        <Image
          src={downloadFolderActiveIcon}
          alt="download-folder-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Download Folder</span>
      </button>

      {/* Rename Folder */}
      <button className="action-button-radio">
        <Image
          src={renameIcon}
          alt="rename-folder"
          width={25}
          className="icon default"
        />
        <Image
          src={renameActiveIcon}
          alt="rename-folder-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Rename Folder</span>
      </button>

      {/* Delete Folder */}
      <button className="delete-button-radio">
        <Image
          src={deleteIcon}
          alt="delete-folder"
          width={25}
          className="icon default"
        />
        <Image
          src={deleteActiveIcon}
          alt="delete-folder-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Delete Folder</span>
      </button>
    </div>
  );
}
