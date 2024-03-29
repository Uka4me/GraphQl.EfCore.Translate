﻿namespace GraphQl.EfCore.Translate
{

    public enum Comparison
    {
        // Both
        Equal,
        In,
        // NotIn,

        // Object/ List
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,

        // String
        StartsWith,
        EndsWith,
        Contains,
        IndexOf/*,
        Like*/
    }
}