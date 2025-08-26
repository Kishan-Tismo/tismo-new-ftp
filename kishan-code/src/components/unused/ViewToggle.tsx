import { useState } from "react";

export default function ViewToggle() {
  const [selectedView, selSelectedView] = useState("");

  return (
    <div className="flex gap-2">
      <button className="list-view-button">List</button>
      <button className="grid-view-button">Grid</button>
    </div>
  );
}
