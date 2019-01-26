// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar CSharpPreprocessorParser;

options { tokenVocab=CSharpLexer; }

@parser::members
{conditions =[True];
ConditionalSymbols ={"DEBUG":1};

def allConditions(self):
    for c in self.conditions:
        if not c:
            return False;
    return True;        
}	 

preprocessor_directive returns [boolean value]
	: DEFINE CONDITIONAL_SYMBOL directive_new_line_or_sharp{ConditionalSymbols[$CONDITIONAL_SYMBOL.text]=1;
$value = self.allConditions(); } #preprocessorDeclaration

	| UNDEF CONDITIONAL_SYMBOL directive_new_line_or_sharp{del ConditionalSymbols[$CONDITIONAL_SYMBOL.text];
$value = allConditions(); } #preprocessorDeclaration

	| IF expr=preprocessor_expression directive_new_line_or_sharp
	  {$value = $expr.value == "True" and self.allConditions(); 
conditions.append($expr.value == "True"); }
	  #preprocessorConditional

	| ELIF expr=preprocessor_expression directive_new_line_or_sharp
	  {if (len(conditions)>0):
    conditions.pop(); 
    $value = $expr.value == "True" and  self.allConditions();
    conditions.append($expr.value == "True");  
else:
    $value = false; }
	     #preprocessorConditional

	| ELSE directive_new_line_or_sharp
	  {if (len(conditions)>0):
    conditions.pop(); 
    $value = True and  self.allConditions(); 
    conditions.append(True);
else:
    $value = False; }    #preprocessorConditional

	| ENDIF directive_new_line_or_sharp             {conditions.pop();
$value = conditions[len(conditions)-1]; }
	   #preprocessorConditional
	| LINE (DIGITS STRING? | DEFAULT | DIRECTIVE_HIDDEN) directive_new_line_or_sharp {$value = self.allConditions(); }
	   #preprocessorLine

	| ERROR TEXT directive_new_line_or_sharp       {$value = self.allConditions(); }   #preprocessorDiagnostic

	| WARNING TEXT directive_new_line_or_sharp     {$value = self.allConditions(); }   #preprocessorDiagnostic

	| REGION TEXT? directive_new_line_or_sharp      {$value = self.allConditions(); }   #preprocessorRegion

	| ENDREGION TEXT? directive_new_line_or_sharp  {$value = self.allConditions(); }   #preprocessorRegion

	| PRAGMA TEXT directive_new_line_or_sharp      {$value = self.allConditions(); }   #preprocessorPragma
	;

directive_new_line_or_sharp
    : DIRECTIVE_NEW_LINE
    | EOF
    ;

preprocessor_expression returns [String value]
	: TRUE                                 {$value = "True"; }
	| FALSE                                {$value = "False"; }
	| CONDITIONAL_SYMBOL                   {$value = ConditionalSymbols.contains($CONDITIONAL_SYMBOL.text) and "True" or "False"; }
	| OPEN_PARENS expr=preprocessor_expression CLOSE_PARENS {$value = $expr.value; }
	| BANG expr=preprocessor_expression     {$value = $expr.value.equals("True") and  "False" or "True"; }
	| expr1=preprocessor_expression OP_EQ expr2=preprocessor_expression
	  {$value = ($expr1.value == $expr2.value and "True" or "False"); }
	| expr1=preprocessor_expression OP_NE expr2=preprocessor_expression
	  {$value = ($expr1.value != $expr2.value and "True" or "False"); }
	| expr1=preprocessor_expression OP_AND expr2=preprocessor_expression
	  {$value = ($expr1.value == "True"  and $expr2.value.equals("True") and "True" or "False"); }
	| expr1=preprocessor_expression OP_OR expr2=preprocessor_expression
	  {$value = ($expr1.value == "True" or $expr2.value.equals("True") and "True" or "False"); }
	;