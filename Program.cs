using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;

namespace AWSUploader
{
    class UploadTest
    {




        public static void Main()
        {
            AWSUtil.Init("meta-bz11-static", "mochengTest", "AKIAU5JU5IZYTYUIFSHX",
                                             "ItxU171wk01tZhl9kol8Mi7VnIwP+tFS/pp1zdca");
            AWSUtil.UploadDir("D:\\de\\AWS\\Video");

        }

      
    }
}