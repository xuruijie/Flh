//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Flh.Business.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Admin
    {
        public long uid { get; set; }
        public System.DateTime created { get; set; }
        public bool enabled { get; set; }
    
        public virtual User User { get; set; }
    }
}
