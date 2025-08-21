export default function FileTable() {
  return (
    <table className="table card">
      <thead>
        <tr>
          <th>Name</th>
          <th>Modified Date</th>
          <th>File Size</th>
          <th>Sort</th>
        </tr>
      </thead>
      <tbody>{/* File rows will go here */}</tbody>
    </table>
  );
}
