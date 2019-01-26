# encoding: utf-8
import sys
from antlr4 import *
from antlr4 import TokenStreamRewriter;
from antlr4.error.ErrorListener import *
from pprint import pprint
import os;
import codecs 
from antlr4.InputStream import InputStream
from CSharpLexer import CSharpLexer
from CSharpParser import CSharpParser
from CSharpParserListener import CSharpParserListener
from CSharpPreprocessorParser import CSharpPreprocessorParser;
from antlr4.Parser import TraceListener;
import thread  
import threading 
import time 
import random
import json;
from multiprocessing import Pool
import multiprocessing;
class VerboseListener(ErrorListener) :
    def syntaxError(self, recognizer, offendingSymbol, line, column, msg, e):
        stack = recognizer.getRuleInvocationStack()
        stack.reverse()
        print("rule stack: ", str(stack))
        print("line", line, ":", column, "at", offendingSymbol, ":", msg)
'''        
class MyListener:
    def enterClass_body(self, ctx):
        print("class:",ctx,ctx.start.line,ctx.start.text,ctx.children);
    def enterMethod_invocation(self, ctx):    
        print("method enter:",ctx.start.line);
    def exitMethod_invocation(self, ctx):
        print("method:",ctx,ctx.start.line,ctx.start.text,ctx.children,ctx.argument_list(),ctx.__unicode__());
        print "ctx:",type(ctx);
        for child in ctx.children :
            print "child:",type(child),child ;
    def enterType_argument_list(self, ctx):
        print("argument:",ctx,ctx.start.line,ctx.start.text,ctx.children,ctx.argument_list(),ctx.__unicode__());
    def enterNamespace_or_type_name(self, ctx):
        print("namespace:",ctx,ctx.start.line,ctx.start.text,ctx.children ,ctx.__unicode__()); 
    def enterIdentifier(self, ctx):
        
        print("Identifier:",ctx,ctx.start.line,ctx.start.text,ctx.children ,ctx.__unicode__()); 
    def exitNamespace_or_type_name(self, ctx):
        print("namespace exit:",ctx,ctx.start.line,ctx.start.text,ctx.children ,ctx.__unicode__()); 
'''    
class RefractListener(ParseTreeListener):
    def __init__(self,tokens,alltpinfo,allWords):
        self.rewriter = TokenStreamRewriter.TokenStreamRewriter(tokens);
        self.newFuncCnt = 0;
        self.alltpinfo = alltpinfo;
        self.allWords = allWords;
        
        self.classNameStack=[];
        self.nsStack=[];
        self.maxFunCnt = 5;
    def enterNamespace_body(self,ctx):
        self.nsStack.append(ctx.parentCtx.qualified_identifier().getText());
    def exitNamespace_body(self,ctx):
        self.nsStack.pop();    
    def enterClass_body(self,ctx):
        ns = ".".join(self.nsStack);
        if ns != "":
            ns = ns+".";
        clsname = ctx.parentCtx.identifier().getText();
        
        if ctx.parentCtx.type_parameter_list() != None and len(ctx.parentCtx.type_parameter_list().type_parameter())>0:
            #print "param:",ctx.parentCtx.type_parameter_list(),ctx.parentCtx.type_parameter_list().type_parameter()
            clsname = clsname+"`"+str(len(ctx.parentCtx.type_parameter_list().type_parameter()));
            #print "clsName:",clsname;
        if len(self.classNameStack)>0:
            clsname=self.classNameStack[len(self.classNameStack)-1]+"+"+clsname;
        else:
            clsname =  ns+clsname
        self.classNameStack.append(clsname);
        #print "classname:",self.classNameStack
    def exitClass_body(self,ctx):
        pass
        
    def exitClass_definition(self, ctx):
        #print "enter class:",type(ctx.start);
        
        #if self.curClass != None:
        #    self.classStack.append(self.curClass);
        #self.curClass={"variables":{},"funcs":{} };
        #self.rewriter.replaceSingleToken(ctx.identifier().start,"testst");
        print "exit class:",len(self.classNameStack),ctx.stop.line;    
        self.classNameStack.pop();
        pass   
    def exitClass_member_declaration(self, ctx):
        
        if   self.newFuncCnt<self.maxFunCnt and random.random()<0.3:
            fun,using = createNewFunc(self.alltpinfo,self.allWords,self.classNameStack[len(self.classNameStack)-1]);
            self.rewriter.insertAfterToken(ctx.stop,"\n"+fun);
            self.newFuncCnt = self.newFuncCnt +1;
        
        pass;
    '''
    def enterClass_member_declaration(self, ctx):
        print "start:",ctx.start.column,ctx.start.tokenIndex, self.rewriter.tokens.tokens[ctx.start.tokenIndex-1].text  ;
        print "token:",ctx.start.text,ctx.start.type,ctx.start.channel,ctx.start.start,ctx.start.stop;
        #self.rewriter.insertBeforeToken( ctx.start,"public int test123;\n");
        
        pass 
    '''        
class PrintListner(ParseTreeListener):
    def __init__(self,fn):
        self.f = open(fn,"w");
    def exitCompilation_unit(self, ctx):
        self.printTree(ctx,0);
        pass
    def writeList(self,*args,**kargs):
        self.f.write(" ".join([  str(x) for x in args]));
        self.f.write("\n");
    def printTree(self,root,indent):
        if root == None or root.children == None:
            return;
        for c in root.children:
            if isinstance(c,tree.Tree.TerminalNodeImpl):
                self.writeList(" "*indent,"term:",c.symbol.text.encode("utf-8"),c.symbol.line,c.symbol.start,c.symbol.stop);
                #self.printTree(c,indent+1);
            elif isinstance(c,CSharpParser.IdentifierContext):
                self.writeList(" "*indent,"id:", c.start.text,c.start.line,c.start,c.stop ,type(c.parentCtx));
                self.printTree(c,indent+1);
            elif isinstance(c,CSharpParser.Class_bodyContext):
                self.writeList(" "*indent,"body:",c,c.start.text.encode("utf-8"),c.start.line );
                self.printTree(c,indent+1);
            else:
                self.writeList(" "*indent, type(c).__name__ ,c.start.text.encode("utf-8"),c.start.line );
                self.printTree(c,indent+1);
class MyListener(ParseTreeListener):
    def enterCompilation_unit(self, ctx):
        self.classes={};
        self.classStack=[];
        self.curClass=None;
    def enterClass_definition(self, ctx):
        if self.curClass != None:
            self.classStack.append(self.curClass);
        self.curClass={"variables":{},"funcs":{} };
        pass    
    def exitClass_definition(self, ctx):
        b = ctx.class_base();
        #print "b:",type(b),self.printTree(b,0);
        
        #print "c:",b.getChild(0,CSharpParser.Class_typeContext).getText();
        #print "exist class:",ctx.start.text,ctx.identifier().getText(),ctx.class_base().getText(),type(ctx.class_base().getChild(CSharpParser.Class_typeContext));
        clsName = ctx.identifier().getText();
        
        baseCls = "";
        if b != None:
            baseCls = b.getChild(0,CSharpParser.Class_typeContext).getText();
        self.curClass["baseClass"] = baseCls;    
        self.classes[clsName] = self.curClass;
        self.curClass = None;
        if len(self.classStack)>0:
            self.curClass = self.classStack.pop();
        
    def printTree(self,root,indent):
        if root == None or root.children == None:
            return;
        for c in root.children:
            if isinstance(c,tree.Tree.TerminalNodeImpl):
                print " "*indent,"term:",c.symbol,c.symbol.line,c.symbol.start,c.symbol.stop;
                #self.printTree(c,indent+1);
            elif isinstance(c,CSharpParser.IdentifierContext):
                print " "*indent,"id:", c.start.text,c.start.line,c.start,c.stop ,type(c.parentCtx)
                self.printTree(c,indent+1);
            elif isinstance(c,CSharpParser.Class_bodyContext):
                print " "*indent,"body:",c,c.start.text,c.start.line 
                self.printTree(c,indent+1);
            else:
                print " "*indent,"unknown:",type(c),c.start.text,c.start.line 
                self.printTree(c,indent+1);
    def getDecl(self,ctx):
        decl = [];
        for c in ctx.children:
            decl.append(c.start.text);
        return ",".join(decl);
    #def getDeclType(self,ctx):
    def getFieldDecl(self,ctx):
        fieldType = ctx.children[0].start.text;
        if isinstance(ctx.children[1],tree.Tree.ErrorNodeImpl):   
            print "error:",ctx.children[1].getText()
            return fieldType,ctx.children[1].getText(),False;
        return fieldType,ctx.children[1].start.text,isinstance(ctx.children[1],CSharpParser.Field_declarationContext)
        pass;
    def enterClass_member_declaration(self, ctx):
        self.curMethodCalls = [];
  
    
        
    #ctx:Method_declaration    
    def addMethod(self,ctx,methodTp,modifier):
        
        
        
        name = ctx.method_member_name().getText();    
        #name =  ctx.getTypedRuleContext(CSharpParser.Method_member_nameContext,0).getText();
        params = ctx.formal_parameter_list();
        args=[];
        if params != None:
            if params.fixed_parameters() != None:
                for p in  params.fixed_parameters().fixed_parameter():
                    decl =  p.arg_declaration();
                    tp =  decl.ctype().getText();
                    argname = decl.identifier().getText();
                    args.append({"tp":tp,"name":argname});
        
        print "addMethod param:", name,methodTp,args;
        self.curClass["funcs"][name] =   {"tp":methodTp,"line":ctx.start.line,"modifier":modifier,"args":args}          
    def exitClass_member_declaration(self, ctx):
        modifier = "";
        if ctx.all_member_modifiers() != None:
            modifier = ctx.all_member_modifiers().getText();
        subctx = ctx.common_member_declaration();
        #print "common_member_declaration:",type(subctx),subctx.getText();
        #ctx2 = subctx.typed_member_declaration();
        #if ctx2 != None:
        #    print "ctx2:",ctx2.getText();
        #methodCtx = subctx.method_declaration();
        if subctx.method_declaration() != None:            
            self.addMethod(subctx.method_declaration(),"void",modifier);
        elif subctx.typed_member_declaration() != None:
            ctype = subctx.typed_member_declaration().ctype().getText();
            if  subctx.typed_member_declaration().method_declaration() != None:    
                self.addMethod(subctx.typed_member_declaration().method_declaration(),ctype,modifier);
            
        decl = "private";
        fieldName="";
        fieldType="";
        isVar = False;
        for c in ctx.children:
            if isinstance(c,CSharpParser.All_member_modifiersContext):
                decl=self.getDecl(c);
                #print "decl:",decl;
            elif  isinstance(c,CSharpParser.Common_member_declarationContext):   
                if c.children == None:
                    print "c:",self.printTree(ctx,0); 
                    continue;
                if isinstance(c.children[0] ,CSharpParser.Typed_member_declarationContext):
                    fieldType,fieldName,isVar = self.getFieldDecl(c.children[0]); 
                elif isinstance(c.children[0],tree.Tree.ErrorNodeImpl):   
                    print "error node:",c.getText();
                elif isinstance(c.children[0],tree.Tree.TerminalNodeImpl):   
                    fieldType = c.children[0].symbol.text;
                    fieldName= c.children[1].start.text;
        if isVar:
            self.curClass["variables"][fieldName]={"tp":fieldType,"line":ctx.start.line,"decl":decl};
        else:
            i = 1;
            origFieldName = fieldName;
            while fieldName in self.curClass["funcs"]:
                fieldName = origFieldName+u"_"+str(i);
                i = i+1;
            self.curClass["funcs"][fieldName] = {"tp":fieldType,"line":ctx.start.line,"decl":decl,"calls":self.curMethodCalls,ctx:ctx};
        self.curMethodCalls = [];        
        '''
        child1 = ctx.children[0];
        child2 =ctx.children[1];
        varName = child2.start.text;
        tp = child1.start.text;
        if isinstance(child2,CSharpParser.Field_declarationContext):
            self.curClass["variables"][varName]={"tp":tp,"line":ctx.start.line};
            print "field:",varName,tp;
        elif isinstance(child2,CSharpParser.Method_declarationContext):    
            self.curClass["funcs"][varName] = {"tp":tp,"line":ctx.start.line};
            print "func:",varName,tp;
        '''    
        #print "var:",decl,fieldType,fieldName,isVar;
    def hasMethodCall(self,ctx):
        for c in ctx.children:
            if isinstance(c,CSharpParser.Method_invocationContext):   
                return True;
        return False;        
    def exitPrimary_expression(self, ctx):
        if not self.hasMethodCall(ctx):
            return;
        '''    
        print "exp:",ctx.start,ctx.stop,ctx.toString(CSharpParser.ruleNames,None),ctx.getText();   
        t = ctx.children[0];
        s = t.children[0].children[0].symbol;
        s.text="TestSSS";
        c = ctx;
        print "exp symbole:",s.text,type(c),c.getText(); 
        '''
        self.curMethodCalls.append(ctx);
        pass    
class MyFileStream(InputStream):

    def __init__(self, fileName, encoding='ascii', errors='strict'):
        self.fileName = fileName
        # read binary to avoid line ending conversion
        with open(fileName, 'rb') as file:
            bytes = file.read()
            #BOM
            if bytes[0] == "\xef" and bytes[1] == "\xbb" and bytes[2] == "\xbf":
                data = codecs.decode(bytes, "utf-8-sig", errors)
                
            else:
                print "normal utf8";
                try:
                    data = codecs.decode(bytes, "utf-8", errors)
                except:
                    data = codecs.decode(bytes, "gbk", errors)
                     
            datas = data.split(u"\n");
            newdatas = [];
            ifcnt = 0;
            startelse = False;
            for d in datas:
                '''
                if d.strip().startswith(u"#endif"):
                    ifcnt = ifcnt-1;
                    continue;
                    
                if startelse:                    
                    continue;
                if d.strip().startswith(u"#else"):
                    startelse = True;
                    continue;
                '''    
                if d.strip().startswith(u"#"):
                    data = None;
                    print "ignore preprocessor file:",fileName;
                    super(type(self), self).__init__([])   
                    return;
                    #continue;
                newdatas.append(d);
            data = u"\n".join(newdatas);    
            
            super(type(self), self).__init__(data)     
def parseFile(fn,bPrint):
    istream = MyFileStream(fn,encoding="utf-8-sig")
    lexer = CSharpLexer(istream)
    stream = CommonTokenStream(lexer)
    '''
    p1 = CSharpPreprocessorParser(stream);
    p1.removeErrorListeners()
    p1.addErrorListener(VerboseListener())
    p1.preprocessor_directive();
    '''
    
    parser = CSharpParser(stream)
    parser.removeErrorListeners()
    parser.addErrorListener(VerboseListener())
    listener = MyListener();
    parser.addParseListener(listener);
    if bPrint :
        parser.addParseListener(CSharpParserListener());
        #parser.addParseListener(TraceListener(parser));
    re = parser.compilation_unit();
    if re.exception != None:
        print "re:",re ;
        return False;
    print "re:",type(re);    
    return True;
    #print "unit:", listener.classes
mylock = thread.allocate_lock()     
class ParseJob(threading.Thread):
    def __init__(self,sep,data,srcdir,destdir,alltpinfo):
        threading.Thread.__init__(self)  
        self.data = data;
        self.alltpinfo=alltpinfo;
        self.srcdir = srcdir;
        self.destdir = destdir;
        self.sep = sep;
    def run(self):
        print "parsejobj start:";
        while True:  
            self.sep.acquire()
            if len(self.data) == 0:
                self.sep.release();
                return;
            item = self.data.pop();
            self.sep.release();
            print "begin parse:",item["fn"];
            refractFile(item["fn"],self.srcdir,self.destdir,self.alltpinfo,item["bPrint"]);
            
        print "thread end";
def RefractJob(sep,data,srcdir,destdir,alltpinfo,allWords):     
    print "RefractJob start:";
    while True:  
        sep.acquire()
        if len(data) == 0:
            sep.release();
            return;
        item = data.pop();
        sep.release();
        print "begin parse:",item["fn"];
        refractFile(item["fn"],srcdir,destdir,alltpinfo,allWords,item["bPrint"]);
            
    print "thread end";
def refractFile(fn,srcdir,destdir,alltpinfo,allWords,bPrint):
    print "begin refract file:",fn;
    istream = MyFileStream(fn,encoding="utf-8-sig")
    if istream.size  == 0:
        return;
    lexer = CSharpLexer(istream)
    stream = CommonTokenStream(lexer)
    parser = CSharpParser(stream)
    parser.removeErrorListeners()
    parser.addErrorListener(VerboseListener())
    listener = RefractListener(stream,alltpinfo,allWords);
    parser.addParseListener(listener);
    if bPrint:
        parser.addParseListener(PrintListner("test.txt"));
    re = parser.compilation_unit();
    if re.exception != None:
        print "re:",re ;
        return False;
    print "re:",re;    
    #print "classes:", listener.classes;    
    print "begin write test.cs";  
    newfn = destdir+ fn[len(srcdir):];
    lpos = max(newfn.rfind("/"),newfn.rfind("\\"));
    newdir = newfn[:lpos];
    if not os.path.exists(newdir):
        os.makedirs(newdir);
    print "srcfn:",fn,"newfn:",newfn,newdir;
    with open(newfn,"wb") as f:
        f.write(listener.rewriter.getDefaultText().encode("utf-8"));   
    #re,using = createNewFunc(alltpinfo,"ParticleEffectAni");
    #with open("func.cs","w") as f:
    #    f.write(re);    
    #print "re:",type(re),re,parser._ctx;           
    return True;
def randomSelect(val,rateArr):
    #print "randomSelect:",val;
    for i in range(len(rateArr)):
        val = val - rateArr[i];
        if val<=0:
            return i;
    return len(rateArr) - 1;
compOp = [">","<","==",">=","<=","!="];    
def getIntCond(ctx,argname):
    op = compOp[random.randint(0,len(compOp)-1)];
    return argname+op+str(random.randint(0,100));
def getFloatCond(ctx,argname):
    op = compOp[random.randint(0,len(compOp)-1)];
    return argname+op+str(random.uniform(0,100))+"f";
def getDoubleCond(ctx,argname):
    op = compOp[random.randint(0,len(compOp)-1)];
    return argname+op+str(random.uniform(0,100)) ;
def getBoolCond(ctx,argname): 
    return argname ; 
def getStringCond(ctx,argname): 
    return argname+"=="+argValString(ctx);     
condTpFunc={"int":getIntCond,"float":getFloatCond,"double":getDoubleCond,"bool":getBoolCond,"string":getStringCond};
def condLocal(ctx):
    locals = ctx["locals"];
    if len(locals) == 0:
        return None;
    var = locals[random.randint(0,len(locals)-1)];
    if var["tp"] in condTpFunc:
        return condTpFunc[var["tp"]](ctx,var["name"]);
    else:
        #print "unknown tp for condTpFunc:",var["tp"];
        return None;
def condArgs(ctx):
    locals = ctx["args"];
    if len(locals) == 0:
        return None;
    var = locals[random.randint(0,len(locals)-1)];
    if var["tp"] in condTpFunc:
        return condTpFunc[var["tp"]](ctx,var["name"]);
    else:
        #print "unknown tp for condTpFunc:",var["tp"];
        return None;   
def condMemberVars(ctx):
    clsinfo =  ctx["alltpinfo"][ctx["curClass"]];
    vars = clsinfo["fields"];
    if len(vars) == 0:
        return None;
    elif len(vars) == 1:
        var = vars[0];
    else:
        var = vars[random.randint(0,len(vars)-1)];
     
    if var["tp"] in condTpFunc:
        return condTpFunc[var["tp"]](ctx,var["name"]);
    else:
        print "unknown tp for condTpFunc:",var["tp"];
        return None;   
       
condFunc = [condLocal,condArgs,condMemberVars];
condFuncRate=[0.2,0.2,0.6];    
def createCond(ctx):
    while True:
        idx = randomSelect(random.random(),condFuncRate);
        re =  condFunc[idx](ctx);
        if re != None:
            return re;
        
def createIfStat(ctx,indent,statnum):
    #print "createIfStat start:",statnum;
    re = "\t"*indent+"if("+createCond(ctx)+")\n";
    re = re + "\t"*indent+"{\n";
    statnum = statnum-1;
    localre , statnum =createStats(ctx,indent+1,statnum); 
    re = re + localre;
    re = re + "\t"*indent+"}\n";
    
    #print "createIfStat end:",statnum;
    return re,statnum;
def createForStat(ctx,indent,statnum):
    #print "createForStat start:",statnum;
    fornum = random.randint(1,1000);
    iname = getRandomName(ctx["allWords"],ctx["usedName"]);
    re = "\t"*indent+"for(int "+iname+"=0;"+iname+"<"+str(fornum)+";"+iname+"++)\n";
    re = re + "\t"*indent+"{\n";
    statnum = statnum-1;
    localre , statnum =createStats(ctx,indent+1,statnum); 
    re = re + localre;
    re = re + "\t"*indent+"}\n";
    
    #print "createForStat end:",statnum;
    return re,statnum;  
def createWhileStat(ctx,indent,statnum):
    #print "createWhileStat start:",statnum;
    re = "\t"*indent+"while("+createCond(ctx)+")\n";
    re = re + "\t"*indent+"{\n";
    statnum = statnum-1;
    localre , statnum =createStats(ctx,indent+1,statnum); 
    re = re + localre;
    re = re + "\t"*indent+"}\n";
    
    #print "createWhileStat end:",statnum;
    return re,statnum; 
def createIntOp(ctx,argName):
    return argName +" = "+argName +"+"+ str(random.randint(1,100))+";\n";
def createBoolOp(ctx,argName):
    return argName +" = !"+argName +";\n";    
def createFloatOp(ctx,argName):
    return argName +" = "+argName +"+"+ str(random.uniform(1,100))+"f;\n";
def createDoubleOp(ctx,argName):
    return argName +" = "+argName +"+"+ str(random.uniform(1,100))+";\n";
def createStringOp(ctx,argName):
    return argName +" = "+argName +"+\"str"+ str(random.randint(1,100))+"\";\n"; 
builtinTypeFunc={"int": createIntOp,"float":createFloatOp,"double":createDoubleOp,"string":createStringOp,"bool":createBoolOp,"System.Boolean":createBoolOp,"System.Int32":createIntOp,"System.String":createStringOp,"System.Single":createFloatOp,"System.Double":createDoubleOp};
def createFunArgOp(ctx):
    if len(ctx["args"]) == 1:
        arg = ctx["args"][0];
    else:
        arg = ctx["args"][random.randint(0,len(ctx["args"])-1)];
    func = builtinTypeFunc[arg["tp"]];
    return func(ctx,arg["name"]);
def createLocalVarOp(ctx):
    #print "createLocalVarOp len:",len(ctx["locals"])
    if len(ctx["locals"]) == 0:
        return None;
    if len(ctx["locals"]) == 1:
        arg =ctx["locals"][0];
    else:
        arg = ctx["locals"][random.randint(0,len(ctx["locals"])-1)];
    #print "createLocalVarOp arg",arg    
    func = builtinTypeFunc[arg["tp"]];
    return func(ctx,arg["name"]);    
def createLocalOp(ctx):
    argname =getRandomName(ctx["allWords"],ctx["usedName"]);
    return "int "+argname+" = "+str(random.randint(1,100))+";\n";
def argValInt(ctx):
    return str(random.randint(0,1000));
def argValDouble(ctx):
    return str(random.uniform(0,1000));
def argValFloat(ctx):
    return str(random.uniform(0,1000))+"f";  
def argValString(ctx):
    val="";
    for i in range(random.randint(1,5)):
        val = val +getRandomName(ctx["allWords"]);
    return "\""+val+"\"";   
def argValBool(ctx):
    return random.random()>0.5 and "true" or "false";
baseTypeArgVal = {"System.Int32":argValInt,"System.Int16":argValInt,"System.Double":argValDouble,"System.Single":argValFloat,"int":argValInt,"float":argValFloat,"double":argValDouble,"System.String":argValString,"string":argValString,"bool":argValBool,"System.Boolean":argValBool};    
def getArgVal(cls):
    for f in cls["funcs"]:
        if f["bconstructor"]:
            print "getArgVal constructor:",f;
            return "new "+cls["fullname"]+"()";
    if cls["hasEmptyConstructor"] == False:
        return None;
    return "new "+cls["fullname"]+"()";
    #if cls["bvaluetype"]:
knownGenericTypes={"System.Collections.Generic.KeyValuePair":1,"System.Collections.Generic.Dictionary":1,"System.Collections.Generic.List":1}        
def createMemberFuncOp(ctx):
    alltps = ctx["alltpinfo"];
    clsinfo =  alltps[ctx["curClass"]];
    #print "clsinfo:",clsinfo;
    
    funcs = clsinfo["funcs"];
    if len(funcs) == 0:
        return None;
    elif len(funcs) == 1:
        idx = 0;
    else:
        idx = random.randint(0,len(funcs)-1);
    func = funcs[idx];
    fname = func["name"];
    print "createMemberFuncOp:",ctx["curClass"],fname,func["args"];
    if fname == "Finalize" or  fname.find("<")>=0 or fname.startswith("get_") or fname.startswith("set_" ) or fname.find(".")>0 :
        return None;
    if func["bgeneric"]:
        print "ignore generic func:",fname;
        return None;
    extra="";
    args = [];
    for a in func["args"]:
        atp = a["tp"];
            
        if atp == None or  atp.find("+")>0:
            return None;
        print "atp:",atp,a["genericTps"];    
        isArray = a["barray"];
        '''
        if atp.endswith("[]"):
            atp = atp[:len(atp)-2];
            isArray = True;
            #args.append( "new "+natp+"[1]");
        '''
        if atp.find("`")>0:
            firstTp = atp[:atp.find("`")];
            tpcnt = int(atp[atp.find("`")+1:atp.find("[")]);
            print "firstTp:",firstTp,tpcnt;
            if not firstTp in knownGenericTypes and (not firstTp in alltps or  not alltps[firstTp]["hasEmptyConstructor"]):
                args.append("null");
            elif len(a["genericTps"]) != tpcnt:
                print "genericTps count incorrect:",len(a["genericTps"]) , tpcnt;
                args.append("null");
            else:
                for gt in a["genericTps"]:
                    if not gt in alltps:
                        print "genericTps not in alltps:",gt;
                        return None;
                if isArray:
                    args.append("new "+firstTp+"<"+",".join(a["genericTps"])+">[1]");
                else:
                    args.append("new "+firstTp+"<"+",".join(a["genericTps"])+">()");
        #elif atp.startswith("System.Collections.Generic.List") :
        #    args.append("new System.Collections.Generic.List<"+a["genericTps"][0]+">()");    
        elif atp.startswith("System.Collections.Generic.") :
            args.append("null");
        elif atp.endswith("&"):#out param
            natp = atp[:len(atp)-1];
            if natp in alltps:
                aname = getRandomName(ctx["allWords"],ctx["usedName"]);
                argval = getArgVal(alltps[natp]);
                if argval == None:
                    return None;
                extra =extra+ natp+" "+aname+"="+argval+";\n";
                if a["isout"]:
                    args.append("out "+aname);
                else:
                    args.append("ref "+aname);
            else:
                return None;
        elif atp in alltps:
            cls = alltps[atp];
            if isArray:
                args.append( "new "+atp+"[1]");
            else:
                if a["tp"] in baseTypeArgVal:
                    args.append(baseTypeArgVal[a["tp"]](ctx));
                else:
                    argval = getArgVal(cls);
                    if argval == None:
                        return None;
                    args.append(argval);
            print "arg:",a;
        else:
            return None;
    print "args:",args;
    return extra+func["name"]+"("+",".join(args)+");\n";
def createMemberVarOp(ctx):
    clsinfo =  ctx["alltpinfo"][ctx["curClass"]];
    vars = clsinfo["fields"];
    #print "clsinfo vars:",len(vars);
    if len(vars) == 0:
        return None;
    elif len(vars) == 1:
        idx = 0;
    else:
        idx = random.randint(0,len(vars)-1);
    var = vars[idx];
    if var["breadonly"]:
        return None;
    vname = var["name"];
    if vname.find("<")>=0:
        return None;
    vartp = var["tp"];
    if vartp in builtinTypeFunc:
        func = builtinTypeFunc[vartp];
        return func(ctx,var["name"]);
    else:
        #print "unknown type:",var,vartp;
        return None;
    #print "get var:"+var,",tp:",vartp;
    #return "int "+argname+" = "+str(random.randint(1,100))+";\n"; 
plainStatFun = [createFunArgOp,createLocalOp,createLocalVarOp,createMemberVarOp,createMemberFuncOp];
plainStatFunRate = [0.1,0.1,0.1,0.3,0.4];
plainIdx = 1;
def createPlainStat(ctx,indent,statnum):
    while True:
        funidx = randomSelect(random.random(),plainStatFunRate);
        #funidx = 4;
        funstr = plainStatFun[funidx](ctx);
        if funstr != None:
            break;
    #print "funidx:",funidx;
    re = "\t"*indent+  funstr;
    statnum = statnum-1;
    return re,statnum;     
    '''
    global plainIdx;
    re ="\t"*indent+ "int k = "+str(plainIdx)+";\n" ;
    statnum = statnum-1;
    plainIdx = plainIdx +1;
    print "createPlainStat:",statnum,plainIdx;
    return re,statnum;     
    '''
statType=["if","for","while","plain"];
statFunc=[createIfStat,createForStat,createWhileStat,createPlainStat];
builtinType=["int","float","double","string","bool"];
statFuncRate=[0.2,0.1,0.1,0.6];    
def createStats(ctx,indent,statnum):
    
    if statnum <= 1:
        substatnum = 1;
    else:
        substatnum = random.randint(1,statnum);
    statnum = statnum - substatnum;
    #stattp = statType[random.randint(0,len(statType))];   
    #print "\t"*indent,"createStats start:",substatnum,statnum
    re = "";
    while substatnum>0:    
        #小于2句直接用普通语句
        if substatnum<2:
            funtp = len(statFuncRate) - 1;
        else:
            funcrate = random.random();
            funtp = 0;
            while funcrate>0 and funtp<len(statFuncRate):
                funcrate = funcrate - statFuncRate[funtp];
                if funcrate<=0:
                    break;
                funtp = funtp+1;    
        #print "statFunc start:",substatnum,statType[funtp]
        localre , substatnum = statFunc[funtp](ctx,indent,substatnum);
        #print "statFunc end:",substatnum
        re = re + localre;
    
    #print "\t"*indent,"createStats end:",statnum;
    return re,statnum;
funModifier=["public","private","protected"];    
def createNewFunc( alltpinfo,allWords,curClass):
    if not curClass in alltpinfo:
        print "class not in dict:",curClass;
        return "","";
    cls = alltpinfo[curClass];
    
    using = [];
    usedName={};
    funcName = getRandomName(allWords,usedName);
    funcName = funcName[0].upper()+funcName[1:];
    retType = "void";
    
    if cls["bstatic"]:
        mod = "static "+funModifier[random.randint(0,len(funModifier)-2)];
    elif cls["bseal"]:
        mod =  funModifier[random.randint(0,len(funModifier)-2)];
    else:
        mod = funModifier[random.randint(0,len(funModifier)-1)];
    re = "\t"+mod+"  "+retType+" "+funcName+"(";
    argnum = 2;
    args=[];
    argsdata=[];
    for i in range(argnum):
        
        argtp =builtinType[random.randint(0,len(builtinType)-1)]; 
        argname = getRandomName(allWords,usedName);
        arg = " ".join([argtp,argname]);
        args.append(arg);
        argsdata.append({"tp":argtp,"name":argname});
    re = re+",".join(args)+")\n";
    re = re +"\t{\n";
    ctx={"args":argsdata, "usedName":usedName, "curClass":curClass,"alltpinfo":alltpinfo,"allWords":allWords};
    
    localvalnum = random.randint(0,5);
    localvars={};
    localvarsdata=[];
    localvarstr="";
    for i in range(localvalnum):
        vartp = builtinType[random.randint(0,len(builtinType)-1)]; 
        varname = getRandomName(allWords,usedName);
        var = "\t"*2+" ".join([vartp,varname]);
        localvarstr = localvarstr + var+"="+baseTypeArgVal[vartp](ctx)+";\n";
        localvarsdata.append({"tp":vartp,"name":varname}); 
    re = re + localvarstr;
    statnum = random.randint(1,50);
    statnum = 20;
    #print "create statnum:",statnum;
    ctx["locals"]=localvarsdata;
    while statnum>0:
        #print "loop start:",statnum
        localre,statnum = createStats(ctx,2,statnum);
        re = re + localre;
        #print "loop end:",statnum
    re = re +"\t}\n"; 
    return re,using;
def readTypeInfo():
    f =  open("e:\\workspace\\test\\front1\\main\\Assets\\types.json");
    tpinfo =  json.load(f);
    print "tpinfo:",len(tpinfo.keys());
    return tpinfo;
allWords=[];    
def getRandomName(allWords,exclude=None):
  
    while True:
        idx = random.randint(0,len(allWords)-1);
        idx2 = random.randint(0,len(allWords)-1);
        w = allWords[idx]+ allWords[idx2][0].upper()+allWords[idx2][1:]; 
        if exclude == None or not w in exclude:
            if exclude != None:
                exclude[w] = 1;
            return w;
def readNames():
     
    allWords=[];
    with open("names.txt") as f:
        for l in f.readlines():
            allWords.append(l.strip());
    return allWords;        
def fun1(lock):
    print "fun1";
def main(argv):
    alltpinfo = readTypeInfo();
    allWords = readNames();
    
    #exit(0);
    data = [];
    bStart = True;
    for root,dir,files in os.walk(argv[1]):
        for f in files:
            if f.endswith(".cs"):
                fn = os.path.join(root,f);
                
                data.append({"fn":fn,"bPrint" : len(argv)>2 and argv[2] == "print"});
                
                if fn.find("\\Signal.cs")>0:
                    #refractFile(fn,argv[1],argv[2],alltpinfo,allWords,True);
                    print "find:",fn;
                    bStart = True;
                    #exit(1);
                if not bStart:
                    continue;
                #refractFile(fn,argv[1],argv[2],alltpinfo,False);   
                
                #if not parseFile(fn,len(argv)>2 and argv[2] == "print"):
                #    pass;
                    #exit(1);
                #print "f:",fn;
       
    #exit(1);
    lock = multiprocessing.Lock()
    process=[];
    for i in range(10):
        p =   multiprocessing.Process(target=RefractJob, args=(lock,data,argv[1],argv[2],alltpinfo,allWords ))
        p.start();
        process.append(p);
    for p in process:
        p.join();
     
    
    s = multiprocessing.Semaphore(2)
    print "s:",s;
    pool = Pool(processes=4)  # 创建进程池，指定最大并发进程数
    for i in range(4):
        print "add task:",i;
        pool.apply_async( fun1, args=(lock,))  # 每个进程调用task函数， 用元组的形式传递参数
    print "add pools";
    pool.close()  # 关闭进程池
    pool.join()  # 主进程等待进程池中的进程执行完毕
    '''
    threads = [];
    for i in range(10):
        t = ParseJob(data,argv[1],argv[2],alltpinfo);
        t.start();
        threads.append(t);
    for t in threads:    
        t.join();
    '''    
    #print parser.class_type();
    #print "args:",parser.namespace_or_type_name() 
    #print "method:",parser.method_invocation();

if __name__ == '__main__':
    main(sys.argv)