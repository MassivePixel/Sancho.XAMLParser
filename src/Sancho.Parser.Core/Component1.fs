namespace Sancho.Parser.Core

open System.Xml.Linq

type NamespaceDeclaration = { name : string; ns : string }
type StringProperty = { name : string; ns : string; value : string }
type NodesProperty = { name : string; ns : string; nodes : XamlNode list }
and
    XamlProperty =
    | Namespace of NamespaceDeclaration
    | String of StringProperty
    | Content of string
    | ContentNodes of NodesProperty
and
    XamlNode(name : string, ?ns : string, ?children : XamlNode list, ?properties : XamlProperty list) =
        member this.name = name
        member this.ns = match ns with | Some c -> c | None -> ""
        member this.children: XamlNode list =
            match children with
                | Some c -> c
                | None -> []
        member this.properties: XamlProperty list =
            match properties with
                | Some p -> p
                | None -> []

        new (xname: XName) =
            XamlNode(xname.LocalName, xname.NamespaceName)
        new (xname: XName, children : XamlNode list) =
            XamlNode(xname.LocalName, xname.NamespaceName, children)
        new (xname: XName, properties : XamlProperty list) =
            XamlNode(xname.LocalName, xname.NamespaceName, [], properties)
        new (xname: XName, children : XamlNode list, properties : XamlProperty list) =
            XamlNode(xname.LocalName, xname.NamespaceName, children, properties)
        
        member this.WithChild(node : XamlNode) =
            XamlNode(this.name, this.ns, node :: this.children, this.properties)
            

module XamlProcessor =
    let rec count (node: XamlNode) =
        1 + (node.children |> List.sumBy(fun n -> count n))

    let parseAttributes (el : XElement) =
        el.Attributes()
         |> Seq.map(fun a ->
             match (a.Name.LocalName = "xmlns" || a.Name.NamespaceName = "http://www.w3.org/2000/xmlns/") with
             | true ->
                    Namespace { name = a.Name.LocalName;
                                           ns = a.Value }
             | false ->
                     String { name = a.Name.LocalName;
                              ns = a.Name.NamespaceName;
                              value = a.Value })
         |> Seq.toList
     
    let parseExpandedAttributes (el: XElement) parser =
        el.Elements()
         |> Seq.where(fun e -> e.Name.LocalName.Contains ".")
         |> Seq.map(fun e -> match e.HasElements with
                             | true -> ContentNodes { name = e.Name.LocalName;
                                                      ns = e.Name.NamespaceName;
                                                      nodes = e.Elements()
                                                               |> Seq.map(fun x -> parser x)
                                                               |> Seq.toList }
                             | false -> String { name = e.Name.LocalName; ns = e.Name.NamespaceName; value = e.Value })
         |> Seq.toList

    let parseContent (el : XElement) =
        match el.HasElements with
        | true -> []
        | false -> [Content el.Value ]

    let rec parseXmlElement (el : XElement) : XamlNode = 
        let children =
         el.Elements()
          |> Seq.where(fun xe -> not (el.Name.LocalName.Contains "."))
          |> Seq.map(fun xe -> parseXmlElement(xe))
          |> Seq.toList

        let properties =
         [ parseAttributes el;
           parseExpandedAttributes el parseXmlElement;
           parseContent el ]
          |> List.concat

        XamlNode(el.Name, children, properties)

    let parseXml (xml: string) : XamlNode =
        let doc = XDocument.Parse(xml)
        parseXmlElement(doc.Root)