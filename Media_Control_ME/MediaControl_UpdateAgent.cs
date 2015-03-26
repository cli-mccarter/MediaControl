using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Media_Control_ME
{

    /// <summary>
    /// 3-20-15: A agent library runs nightly through all workspaces
    /// where MediaControl has been installed and update each media to 
    /// keep size/counts up to date  --- Chong Li
    /// </summary>
   

    [kCura.Agent.CustomAttributes.Name("MediaControl UpdateAgent")]
    [System.Runtime.InteropServices.Guid("C0472605-3BCE-49D9-8474-715F0A5EA57E")]
    public class MediaControl_UpdateAgent : kCura.Agent.AgentBase
    {
        

        public override void Execute()
        {
            try { 
                /// Call UpdateMediaControl private method to update fields; run other logics here.
    
                

            
            
            }
            catch (System.Exception ex) {

                RaiseError("Agent can not run due to some error", ex.ToString());
            
            }
        }


        /// Need to implement SQL queries via Relativity helpers class
        /// pass a workspace paramter to the method so, it knows which 
        /// workspace it will run against. 
        /// Question? not sure if the workspaceArtifactID should be passed 
        /// as a method paramter of the Relativity helper class and get it from
        /// the environment?? Not sure if i got it right?

        private static void UpdateMediaControl(Relativity.API.IDBContext eddsDBContext, Int32 workspaceArtifactID)
        {
            string updateSQL = @"
                                SELECT Document.[ArtifactID] 
	INTO #T1 #searchId# where [WorkspaceArtifactID] = @workspaceArtifactID

	DECLARE @documentCount INT, @nativeFileCount INT, @nativeFileSize DECIMAL, @imageFileCount INT, @imageFileSize DECIMAL, @producedImageFileCount INT, @producedImageFileSize DECIMAL, @extractedTextSize NVARCHAR(MAX)
	SELECT @documentCount = COUNT([ArtifactID]) FROM #T1

	SELECT 
		@nativeFileCount = COUNT(A.[ArtifactID]),
		@nativeFileSize = ISNULL(CAST(SUM(F.[Size]) AS DECIMAL), 0)
	FROM [EDDSDBO].[File] F WITH(NOLOCK)
		INNER JOIN [EDDSDBO].[Artifact] A WITH(NOLOCK) ON F.[DocumentArtifactID] = A.[ArtifactID]
		INNER JOIN #T1 T ON A.[ArtifactID] = T.[ArtifactID]
	WHERE 
		F.[Type] = 0
		
	SELECT 
		@imageFileCount = COUNT(A.[ArtifactID]),
		@imageFileSize = ISNULL(CAST(SUM(F.[Size]) AS DECIMAL), 0)
	FROM [EDDSDBO].[File] F WITH(NOLOCK)
		INNER JOIN [EDDSDBO].[Artifact] A WITH(NOLOCK) ON F.[DocumentArtifactID] = A.[ArtifactID]
		INNER JOIN #T1 T ON A.[ArtifactID] = T.[ArtifactID]
	WHERE 
		F.[Type] = 1

	SELECT 
		@producedImageFileCount = COUNT(A.[ArtifactID]),
		@producedImageFileSize = ISNULL(CAST(SUM(F.[Size]) AS DECIMAL), 0)
	FROM [EDDSDBO].[File] F WITH(NOLOCK)
		INNER JOIN [EDDSDBO].[Artifact] A WITH(NOLOCK) ON F.[DocumentArtifactID] = A.[ArtifactID]
		INNER JOIN #T1 T ON A.[ArtifactID] = T.[ArtifactID]
	WHERE 
		F.[Type] = 3

	IF (SELECT COUNT(ArtifactID) FROM [EDDSDBO].[Field] WITH(NOLOCK) WHERE [DisplayName] = 'ExtractedTextSize') > 0
		BEGIN
			DECLARE @sql NVARCHAR(1000)
			DECLARE @parms NVARCHAR(1000)

			SET @sql = N'SELECT @extractedTextSizeTemp = ISNULL(CAST(SUM(D.[ExtractedTextSize]) AS DECIMAL), 0) FROM [EDDSDBO].[Document] D WITH(NOLOCK) INNER JOIN #T1 T ON D.[ArtifactID] = T.[ArtifactID] '
			SET @parms = N'@extractedTextSizeTemp int output'

			EXEC sp_executesql @stmt=@sql, @params=@parms, @extractedTextSizeTemp = @extractedTextSize output
		END
	ELSE
		BEGIN
			SET @extractedTextSize = NULL
		END

	SELECT
		@documentCount [Doc Count],
		@nativeFileCount [Native File Count],
		CAST(@nativeFileSize AS DECIMAL) /1024 /1024 /1024 [Native File Size (GB)],
		@imageFileCount [Image File Count],
		CAST(@imageFileSize AS DECIMAL) /1024 /1024 /1024 [Image File Size (GB)],
		@producedImageFileCount [Produced File Count],
		CAST(@producedImageFileSize AS DECIMAL) /1024 /1024 /1024 [Produced File Size (GB)],
		@imageFileCount + @producedImageFileCount [Total Image File Count],
		CAST(@imageFileSize + @producedImageFileSize AS DECIMAL) /1024 /1024 /1024 [Total Image File Size (GB)],
		@nativeFileCount + @imageFileCount + @producedImageFileCount [Total File Count],
		CAST(@nativeFileSize + @imageFileSize + @producedImageFileSize AS DECIMAL) /1024 /1024 /1024 [Total File Size (GB)],
		CAST(@extractedTextSize AS DECIMAL) /1024 /1024 [Extracted Text Size (GB)]

	    DROP TABLE #T1
        
        /* Update corresponding fields with variables  Two questions here 1) MediaControl is an object type, so is it eddsdbo.MediaControl or sth else?
           I think it might be better to add a timestamp field to the MediaControl? */

        INSERT INTO [EDDSDBO].[MediaControl] ([Native File Count] , [Native File Size], 
        [Image File Count],[Image File Size],[Production Image File Count],[Production Image File Size]) VALUES (@nativeFileCount,@nativeFileSize,@imageFileCount,@imageFileSize
        ,@producedImageFileCount,@producedImageFileSize)       
                                                                              ";
            SqlParameter workspaceArtifactIDParam = new SqlParameter("@workspaceArtifactID", SqlDbType.Int);
            workspaceArtifactIDParam.Value = workspaceArtifactID;

        }

        public override string Name
        {
            get { return "MediaControl UpdateAgent"; }
        }
    }
}
