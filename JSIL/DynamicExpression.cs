﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using JSIL.Transforms;

namespace JSIL.Expressions {
    public interface IDynamicExpressionVisitor<T, S> {
        S VisitDynamicExpression (DynamicExpression dynamicExpression, T data);
    }

    /// <summary>
    /// dynamic target: target.MemberName(...) / target.MemberName
    /// </summary>
    public class DynamicExpression : Expression {
        public static readonly Role<Expression> CallSiteRole = new Role<Expression>("CallSite", Null);
        public static readonly Role<Expression> TargetTypeRole = new Role<Expression>("TargetType", Null);

        public CallSiteType CallSiteType;

        public Expression CallSite {
            get { return GetChildByRole(CallSiteRole); }
            set { SetChildByRole(CallSiteRole, value); }
        }

        public Expression TargetType {
            get { return GetChildByRole(TargetTypeRole); }
            set { SetChildByRole(TargetTypeRole, value); }
        }

        public Expression Target {
            get { return GetChildByRole(Roles.TargetExpression); }
            set { SetChildByRole(Roles.TargetExpression, value); }
        }

        public string MemberName {
            get {
                var child = GetChildByRole(Roles.Identifier);
                return child.Name;
            }
            set {
                if (value != null)
                    SetChildByRole(Roles.Identifier, new Identifier(value, AstLocation.Empty));
                else
                    SetChildByRole(Roles.Identifier, Identifier.Null);
            }
        }

        public AstNodeCollection<Expression> Arguments {
            get { return GetChildrenByRole(Roles.Argument); }
        }

        public AstNodeCollection<AstType> TypeArguments {
            get { return GetChildrenByRole(Roles.TypeArgument); }
        }

        public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data) {
            var dcv = visitor as IDynamicExpressionVisitor<T, S>;
            if (dcv != null)
                return dcv.VisitDynamicExpression(this, data);
            else
                return default(S);
        }

        public override bool DoMatch (AstNode other, Match match) {
            DynamicExpression o = other as DynamicExpression;
            return o != null && 
                this.Target.DoMatch(o.Target, match) && 
                MatchString(this.MemberName, o.MemberName) && 
                this.TypeArguments.DoMatch(o.TypeArguments, match);
        }
    }
}
