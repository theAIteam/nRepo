﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AI.nRepo
{
    public interface IDatabasePlatform
    {
        object AsNHibernateConfiguration(string connectionString);


    }
}
