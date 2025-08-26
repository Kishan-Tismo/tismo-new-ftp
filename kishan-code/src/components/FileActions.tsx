import Image from "next/image";

import newFileIcon from "@/assets/new_folder.png";
import newFileActiveIcon from "@/assets/new_folder_active.png";
import uploadFileIcon from "@/assets/upload_file.png";
import uploadFileActiveIcon from "@/assets/upload_file_active.png";
import downloadFileIcon from "@/assets/download_file.png";
import downloadFileActiveIcon from "@/assets/download_file_active.png";
import renameIcon from "@/assets/edit.png";
import renameActiveIcon from "@/assets/edit_active.png";
import deleteIcon from "@/assets/delete.png";
import deleteActiveIcon from "@/assets/delete_active.png";

export default function FileActions() {
  return (
    <div className="action-panel">
      <h3 className="black-text">Actions</h3>

      {/* New File */}
      <button className="new-button-radio">
        <Image
          src={newFileIcon}
          alt="new-file"
          width={25}
          className="icon default"
        />
        <Image
          src={newFileActiveIcon}
          alt="new-file-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">New</span>
      </button>

      {/* Upload File */}
      <button className="action-button-radio">
        <Image
          src={uploadFileIcon}
          alt="upload-file"
          width={25}
          className="icon default"
        />
        <Image
          src={uploadFileActiveIcon}
          alt="upload-file-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Upload</span>
      </button>

      {/* Download File */}
      <button className="action-button-radio">
        <Image
          src={downloadFileIcon}
          alt="download-file"
          width={25}
          className="icon default"
        />
        <Image
          src={downloadFileActiveIcon}
          alt="download-file-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Download</span>
      </button>

      {/* Rename File */}
      <button className="action-button-radio">
        <Image
          src={renameIcon}
          alt="rename-file"
          width={25}
          className="icon default"
        />
        <Image
          src={renameActiveIcon}
          alt="rename-file-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Rename</span>
      </button>

      {/* Delete File */}
      <button className="delete-button-radio">
        <Image
          src={deleteIcon}
          alt="delete-file"
          width={25}
          className="icon default"
        />
        <Image
          src={deleteActiveIcon}
          alt="delete-file-active"
          width={25}
          className="icon hover"
        />
        <span className="small-button-text">Delete</span>
      </button>
    </div>
  );
}
