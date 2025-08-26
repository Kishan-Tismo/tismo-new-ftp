import Image from "next/image";
import { useState, useEffect } from "react";
import { useFolder } from "@/contexts/FolderContext";
import infoIcon from "@/assets/info_icon.png";
import db from "@/assets/db_icon.png";
import { getFileList } from "@/services/api";
import { getFileListTest } from "@/services/MockApi";
import { useAuth } from "@/contexts/AuthContext";
export default function DirectorySelector() {
  const [selected, setSelected] = useState("tismo");
  const { setCurrentFolder } = useFolder();
  const [clientName, setClientName] = useState("Client");
  const { token } = useAuth();

  useEffect(() => {
    const fetchClientName = async () => {
      if (!token) return;
      try {
        // const files = await getFileList("/", token);
        // // Find the first folder (or any folder you want to use as clientName)
        // const folder = files.find((item) => item.isFolder);
        // if (folder && folder.name) {
        //   setClientName(folder.name);
        // }

        // Test API
        const files = await getFileListTest("/", token);
        // Find the first folder (or any folder you want to use as clientName)
        const folder = files.find((item) => item.isFolder);
        if (folder && folder.name) {
          setClientName(folder.name);
        }
      } catch {
        // fallback to default
        setClientName("Client");
      }
    };
    fetchClientName();
  }, [token]);

  // Update currentFolder when selected changes
  useEffect(() => {
    if (selected === "tismo") {
      setCurrentFolder("/From Tismo/");
    } else if (selected === "client") {
      setCurrentFolder(`/From ${clientName}/`);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selected, clientName]);

  return (
    <div className="directory-container">
      <div className="directory-selector">
        <span className="black-text">Main Directory:</span>
        <div className="button-radio-group">
          <button
            className={`button-radio ${selected === "tismo" ? "active" : ""}`}
            onClick={() => setSelected("tismo")}
            title="Tismo Directory"
          >
            <Image src={db} alt="db" width={40} />
            <span className="button-text">Tismo</span>
          </button>

          <button
            className={`button-radio ${selected === "client" ? "active" : ""}`}
            onClick={() => setSelected("client")}
            title="Client Directory"
          >
            <Image src={db} alt="db" width={40} />
            <span className="button-text">{clientName}</span>
          </button>
        </div>
      </div>
      <div className="info-icon" title="Help">
        <Image src={infoIcon} alt="info-icon" width={30} />
      </div>
    </div>
  );
}
