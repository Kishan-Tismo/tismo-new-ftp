import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import sanitize from "sanitize-filename";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function sanitizeFileName(fileName: string): string {
  // sanitize() will replace illegal characters with ''
  const sanitized = sanitize(fileName);
  if (sanitized === "") {
    // If the name is all illegal characters, we need to provide a default
    return "renamed-file";
  }
  return sanitized;
}
