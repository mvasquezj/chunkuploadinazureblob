import './App.css'
import { FileUpload } from './components/FileUpload'

function App() {
  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Azure Blob Storage File Upload</h1>
        <p>Upload files directly to Azure Blob Storage using SAS tokens</p>
      </header>
      <main>
        <FileUpload />
      </main>
    </div>
  )
}

export default App
