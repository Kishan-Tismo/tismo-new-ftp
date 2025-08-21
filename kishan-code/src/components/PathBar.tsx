import Image from "next/image";
import backButtonIcon from "@/assets/back_icon.png";
import copyIcon from "@/assets/copy_icon.png";

export default function PathBar() {
  return (
    <div className="path-container">
      <button className="back-button" title="Go to previous folder">
        <Image src={backButtonIcon} width={30} alt="back-button" />
      </button>
      <div className="path-description">
        <span className="black-text">Path:</span>
        <input className="path-input" id="pathInput" defaultValue={"hello"} />
      </div>
      <div className="info-icon" title="Copy path to clipboard">
        <Image src={copyIcon} alt="copy-icon" width={30} />
      </div>
    </div>
  );
}
