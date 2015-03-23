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
                /// Call UpdateMediaControl private method to update fields; run other logis here.
    
                

            
            
            }
            catch (System.Exception ex) {

                RaiseError("Agent can not run due to some error", ex.ToString());
            
            }
        }


        /// Need to implement SQL queries via Relativity helpers class
        /// pass a workspace paramter to the method so, it knows which 
        /// workspace it will run against
        
        private static void UpdateMediaControl(Relativity.API.IDBContext eddsDBContext, string workspaceid)
        {
            

        }

        public override string Name
        {
            get { return "MediaControl UpdateAgent"; }
        }
    }
}
