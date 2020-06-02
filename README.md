# Uneti.NET

## Overview

`Uneti.NET` lets you quickly and easily compare XML documents.  Instead of doing a line-by-line comparison, `Uneti.NET` checks the structure of the XML, so, as long as all the data is present, the order does not matter.

## Setup

### Initialization

All you need to do to get started with `Uneti.NET` is to initialize an `XmlComparer`. 

    var xmlComparer = new XmlComparer();
    
### Configuration

Once you have an `XmlComparer` initialized, you can configure how it parses XML documents and returns results.

#### Input

You can filter out specific elements when the XML is being read by the parser.  By setting the `Predicate`, any `XElement` that does not meet the specified criteria will be ignored.

    <star_wars>
        <movie>
           <episode>IV</episode>
           <title>A New Hope</title>
            <release_date>05/25/1977</release_date>
        </movie>
        <movie>
            <episode>V</episode>
            <title>The Empire Strikes Back</title>
            <release_date>05/21/1980</release_date>
        </movie>
        <movie>
            <episode>VI</episode>
            <title>Return of the Jedi</title>
            <release_date>05/25/1983</release_date>
        </movie>
    </star_wars>

The following code would prevent the elements with the name *title* from being parsed.

    xmlComparer.Predicate = element => element.Name.LocalName == "title";
    
#### Output

The results can also be modified by setting `ExcludeEmptyNodes`.  An XML node is considered empty if it has no children nor properties.  Since these nodes contain no data, adding or removing them may be seen as inconsequential.  By default empty nodes are included in the results, i.e. they will be considered as edits.

    xmlComparer.ExcludeEmptyNodes = true;
   
   ## Edits
   Now that the setup is complete, comparing two XML documents can be done by calling `GetEdits`.

    var expected = File.ReadAllText("./expected.xml");
    var actual = File.ReadAllText("./actual.xml");
    var edits = xmlComparer.GetEdits(expected, actual);
    
Each `XmlEdit` is composed of three parts: the actual element, the expected element and the edit operation.

### Edit Operations
| Name |Definition | Actual Is Null | Expected Is Null |
|-|-|:-:|:-:|
| Added | The actual element was added. |  | ✓ |
| Modified | The expected element was transformed into the actual element. |  |  |
| Removed | The expected element was removed. | ✓ |  |

### Example

Comparing the above XML to the XML below:

    <star_wars>
        <movie>
           <title>A New Hope</title>
            <release_date>05/25/1977</release_date>
        </movie>
        <movie>
            <episode>VI</episode>
            <title>Return of the Jedi</title>
            <release_date>05/25/1983</release_date>
            <rating>4.7/5.0</rating>
        </movie>
         <movie>
            <episode>V</episode>
            <title>Empire Strikes Back</title>
            <release_date>05/21/1980</release_date>
        </movie>
    </star_wars>

 - `<episode>IV</episode>` (line 3) was removed 
 - `<title>The Empire Strikes Back</title>` (line 9) was modified to `<title>Empire Strikes Back</title>` (line 14)
 - `<rating>4.7/5.0</rating>` (line 10) was added
