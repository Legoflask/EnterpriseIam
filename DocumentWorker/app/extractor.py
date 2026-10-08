from pypdf import PdfReader

def extract_text_from_pdf(file_path: str) -> str:
    """
    Opens a localized physical file tracking path, iterates through individual text blocks,
    and returns a clean serialized text data stream string wrapper.
    """
    try:
        reader = PdfReader(file_path)
        compiled_text_blocks = []

        for page in reader.pages:
            text_slice = page.extract_text()
            if text_slice:
                compiled_text_blocks.append(text_slice)

        # Join paragraphs into a single unified clean structural text payload
        return "\n".join(compiled_text_blocks).strip()

    except Exception as ex:
        raise RuntimeError(f"Failed to extract document binary arrays safely: {str(ex)}")