import os
import shutil
from uuid import UUID
from fastapi import FastAPI, UploadFile, File, Form, HTTPException, status
from extractor import extract_text_from_pdf
from database import update_document_extracted_data

app = FastAPI(
    title="Enterprise Document Extraction Worker",
    version="1.0.0"
)

UPLOAD_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "../storage/uploads"))
os.makedirs(UPLOAD_DIR, exist_ok=True)

@app.post("/api/documents/extract", status_code=status.HTTP_200_OK)
async def process_document_extraction(
    document_id: UUID = Form(...), # 🚀 Passed from your .NET Core API database tracker
    file: UploadFile = File(...)
):
    """
    Accepts an existing tracker ID, saves the PDF chunk to local disk,
    parses its contents, and commits it directly back to SQL Server tables via pyodbc.
    """
    if not file.filename.lower().endswith(".pdf"):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Unsupported file extension. This worker strictly parses PDF documents."
        )

    saved_filename = f"{document_id}.pdf"
    file_path = os.path.join(UPLOAD_DIR, saved_filename)

    try:
        # Save file to physical local storage path
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        # Execute text extraction processing logic
        extracted_content = extract_text_from_pdf(file_path)

        # 🚀 Direct Cross-Framework Sync: Write straight back into your SQL Server DB!
        update_document_extracted_data(str(document_id), extracted_content)

        return {
            "documentId": document_id,
            "status": "Completed",
            "message": "Successfully extracted text and synchronized database states back to SQL Server."
        }

    except Exception as ex:
        if os.path.exists(file_path):
            os.remove(file_path)
            
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Database synchronization or extraction failure: {str(ex)}"
        )