import pyodbc
from config import settings

def update_document_extracted_data(document_id: str, raw_text: str):
    """
    Connects to the primary SQL Server instance via pyodbc, updates the target 
    document status to 'Completed', and logs the extracted text safely using parameterized queries.
    """
    conn_str = settings.connection_string
    
    # 1. Open a native connection channel to SQL Server
    with pyodbc.connect(conn_str) as conn:
        with conn.cursor() as cursor:
            
            # 2. Check if a record already exists in ExtractedData
            cursor.execute("SELECT COUNT(1) FROM ExtractedData WHERE DocumentId = ?", document_id)
            row = cursor.fetchone()
            
            # Extract the first column integer value from the row tuple safely
            record_count = row[0] if row else 0

            if record_count > 0:
                # Update existing extraction data row
                cursor.execute(
                    """
                    UPDATE ExtractedData 
                    SET RawText = ?, ProcessedAtUtc = GETUTCDATE() 
                    WHERE DocumentId = ?
                    """,
                    raw_text, document_id
                )
            else:
                # ✅ FIX: Match the exact 4 columns to the exact 4 values dynamically
                cursor.execute(
                    """
                    INSERT INTO ExtractedData (Id, DocumentId, RawText, ProcessedAtUtc)
                    VALUES (NEWID(), ?, ?, GETUTCDATE())
                    """,
                    document_id, raw_text
                )
            
            # 3. Update the primary matching Document entity tracking state status to 'Completed'
            cursor.execute(
                """
                UPDATE Documents 
                SET Status = 'Completed' 
                WHERE Id = ?
                """,
                document_id
            )
            
            # Commit the internal transaction pool blocks cleanly
            conn.commit()