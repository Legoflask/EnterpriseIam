import os
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    # Leverages your exact SQL Server connection parameters
    DB_SERVER: str = "(localdb)\\mssqllocaldb"
    DB_NAME: str = "EnterpriseIamDb"
    
    @property
    def connection_string(self) -> str:
        return (
            f"DRIVER={{ODBC Driver 17 for SQL Server}};"
            f"SERVER={self.DB_SERVER};"
            f"DATABASE={self.DB_NAME};"
            f"Trusted_Connection=yes;"
            f"TrustServerCertificate=yes;"
        )

settings = Settings()