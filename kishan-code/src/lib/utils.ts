import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Sanitizes a file or folder name by removing or rejecting invalid characters.
 * Returns the sanitized name. If the name contains invalid characters, returns a sanitized version.
 * If you want to only allow valid names, compare the return value to the input.
 */
export function sanitizeFileName(name: string): string {
  // Windows reserved characters: \\ / : * ? " < > |
  // Also disallow control chars and leading/trailing spaces or dots
  // Empty or "." or ".." are not allowed
  const invalidPattern = /[\\/:*?"<>|\x00-\x1F]/g;
  let sanitized = name.replace(invalidPattern, "");
  sanitized = sanitized.replace(/^\s+|\s+$/g, ""); // trim spaces
  sanitized = sanitized.replace(/^\.+|\.+$/g, ""); // trim dots
  if (sanitized === "" || sanitized === "." || sanitized === "..") {
    return "";
  }
  return sanitized;
}
