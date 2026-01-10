import { useState, type ChangeEvent, type FormEvent } from 'react';
import { blobService, type BlobUploadResponse } from '../services/blobService';
import './FileUpload.css';

export function FileUpload() {
    const [file, setFile] = useState<File | null>(null);
    const [uploading, setUploading] = useState(false);
    const [response, setResponse] = useState<BlobUploadResponse | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
        const selectedFile = e.target.files?.[0];
        if (selectedFile) {
            setFile(selectedFile);
            setError(null);
        }
    };

    const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!file) {
            setError('Please select a file');
            return;
        }

        setUploading(true);
        setError(null);
        setResponse(null);

        try {
            const uploadResponse = await blobService.uploadFile(file);
            setResponse(uploadResponse);

            if (uploadResponse.success) {
                setFile(null);
            } else {
                setError(uploadResponse.message);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'An error occurred');
        } finally {
            setUploading(false);
        }
    };

    return (
        <div className="file-upload-container">
            <h2>Upload File to Azure Blob Storage</h2>

            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="file-input">Select File:</label>
                    <input
                        id="file-input"
                        type="file"
                        onChange={handleFileChange}
                        disabled={uploading}
                    />
                    {file && <p className="file-name">Selected: {file.name}</p>}
                </div>

                <button type="submit" disabled={!file || uploading}>
                    {uploading ? 'Uploading...' : 'Upload File'}
                </button>
            </form>

            {error && (
                <div className="alert alert-error">
                    <strong>Error:</strong> {error}
                </div>
            )}

            {response && (
                <div className={`alert ${response.success ? 'alert-success' : 'alert-error'}`}>
                    <strong>{response.success ? 'Success!' : 'Failed!'}</strong> {response.message}
                    {response.blobName && (
                        <p className="blob-name">Blob Name: {response.blobName}</p>
                    )}
                </div>
            )}
        </div>
    );
}
