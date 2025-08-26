import { PathNode } from "@/lib/types";

// Test API that mimics getFileList but returns mock data
export async function getFileListTest(
  _folder: string,
  _token: string
): Promise<PathNode[]> {
  // Simulate network delay
  await new Promise((resolve) => setTimeout(resolve, 300));
  // Return mock data
  return [
    {
      name: "Kern Laser Systems",
      isFolder: true,
      size: 0,
      createdOn: new Date(),
    },
    {
      name: "example.txt",
      isFolder: false,
      size: 1234,
      createdOn: new Date(),
    },
    {
      name: "Documents",
      isFolder: true,
      size: 0,
      createdOn: new Date(),
    },
  ];
}
