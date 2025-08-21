import Image from "next/image";
import { useState } from "react";

import infoIcon from "@/assets/info_icon.png";
import db from "@/assets/db_icon.png";

export default function DirectorySelector() {
  const [selected, setSelected] = useState("tismo");

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
            <span className="button-text">Kern Laser Systems</span>
          </button>
        </div>
      </div>
      <div className="info-icon" title="Help">
        <Image src={infoIcon} alt="info-icon" width={30} />
      </div>
    </div>
  );
}
