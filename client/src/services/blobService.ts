import { BlockBlobClient } from "@azure/storage-blob";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface BlobUploadResponse {
  success: boolean;
  blobName: string;
  message: string;
}

export class BlobService {
  async getSasTokenUri(blobName: string): Promise<string> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/sas`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ blobName: blobName }),
      });

      if (!response.ok) {
        throw new Error(`Failed to get SAS token: ${response.statusText}`);
      }

      const data = await response.text();
      return data;
    } catch (error) {
      console.error("Error getting SAS token:", error);
      throw error;
    }
  }

  async uploadFile(file: File): Promise<BlobUploadResponse> {
    try {
      // Obtener el URI con SAS token
      const blobName = `${Date.now()}-${file.name}`;
      const sasTokenUri = await this.getSasTokenUri(blobName);

      // Crear cliente de blob con el URI de SAS
      const client = new BlockBlobClient(sasTokenUri);

      // Subir el archivo
      await client.uploadData(file, {
        blockSize: 50 * 1024 * 1024, // 50MB chunks
        concurrency: 4,
        maxSingleShotSize: 50 * 1024 * 1024,
        blobHTTPHeaders: {
          blobContentType: file.type
        },
        onProgress: (progress) => {
          console.log(`Uploaded ${progress.loadedBytes} of ${file.size} bytes`);
        },
      });

      return {
        success: true,
        blobName: blobName,
        message: `File ${file.name} uploaded successfully`,
      };
    } catch (error) {
      console.error("Error uploading file:", error);
      return {
        success: false,
        blobName: "",
        message: error instanceof Error ? error.message : "Unknown error",
      };
    }
  }
}

export const blobService = new BlobService();
