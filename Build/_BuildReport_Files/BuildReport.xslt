<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="html" indent="yes" encoding="utf-8"/>

  <xsl:key name="projects-by-file" match="//project" use="@file" />

  <xsl:template match="build" mode="buildXML">
    <!-- build a list of unique projects -->
    <projects>
      <xsl:for-each select=".//project[count(. | key('projects-by-file', @file)) = 1]">
        <project>
          <xsl:variable name="pathSplitSeparator">
            <xsl:text>\</xsl:text>
          </xsl:variable>

          <xsl:attribute name="is-solution">
            <xsl:value-of select="contains(concat(@file,'$$$'), '.sln$$$')"/>
          </xsl:attribute>

          <xsl:variable name="localProjectName" select="@file"/>

          <!-- Sometimes it is possible to have project name set to a path over a real project name,
                  we split the string on '\' and if we end up with >1 part in the resulting tokens set
                  we format the ProjectDisplayName as ..\prior\last -->
          <xsl:variable name="pathTokens">
            <xsl:call-template name="SplitString">
              <xsl:with-param name="source" select="$localProjectName"/>
              <xsl:with-param name="separator" select="$pathSplitSeparator"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="projectName">
            <xsl:value-of select="msxsl:node-set($pathTokens)/part[last()]"/>
          </xsl:variable>

          <xsl:attribute name="file">
            <xsl:value-of select="@file"/>
          </xsl:attribute>

          <xsl:attribute name="name">
            <xsl:value-of select="$projectName"/>
          </xsl:attribute>

          <xsl:attribute name="display-name">

            <xsl:choose>
              <xsl:when test="count(msxsl:node-set($pathTokens)/part) &gt; 1">
                <xsl:value-of select="concat('..', $pathSplitSeparator, msxsl:node-set($pathTokens)/part[last() - 1], $pathSplitSeparator, msxsl:node-set($pathTokens)/part[last()])"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$localProjectName"/>
              </xsl:otherwise>
            </xsl:choose>

          </xsl:attribute>
          <xsl:attribute name="safe-name">
            <!--<xsl:value-of select="translate($projectName, '\', '-')"/>-->
            <xsl:value-of select="generate-id(.)"/>
          </xsl:attribute>

          <messages>
            <xsl:for-each select="//project[@file = current()/@file]/target">
              <xsl:copy-of select="message|warning|error"/>
            </xsl:for-each>
          </messages>
        </project>
      </xsl:for-each >
    </projects>
  </xsl:template>

  <xsl:template match="project" mode="summary">
    <tr>
      <td>
        <xsl:value-of select="@display-name"/>
      </td>
      <td>
        <a>
          <xsl:attribute name="href">#<xsl:value-of select="@safe-name"/>-errors</xsl:attribute>
          <xsl:value-of select="count(messages/error)" />
        </a>
      </td>
      <td>
        <a>
          <xsl:attribute name="href">#<xsl:value-of select="@safe-name"/>-warnings</xsl:attribute>
          <xsl:value-of select="count(messages/warning)" />
        </a>
      </td>
      <td>
        <a>
          <xsl:attribute name="href">#<xsl:value-of select="@safe-name"/>-messages</xsl:attribute>
          <xsl:value-of select="count(messages/message)" />
        </a>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="project" mode="detail">
    <div>
      <h3>
        <xsl:value-of select="@display-name"/>
      </h3>
      <!-- anchor -->
      <a>
        <xsl:attribute name="name"><xsl:value-of select="@safe-name"/>-errors</xsl:attribute>
      </a>
      <ul class="error">
        <xsl:apply-templates select="messages/error" />
      </ul>
      <!-- anchor -->
      <a>
        <xsl:attribute name="name"><xsl:value-of select="@safe-name"/>-warnings</xsl:attribute>
      </a>
      <ul class="warning">
        <xsl:apply-templates select="messages/warning" />
      </ul>
      <!-- anchor -->
      <a>
        <xsl:attribute name="name"><xsl:value-of select="@safe-name"/>-messages</xsl:attribute>
      </a>
      <ul class="message collapsed">
        <xsl:apply-templates select="messages/message" />

        <xsl:variable name="messageCount" select="count(messages/message)"/>
        <xsl:if test="$messageCount != 0">
          <li class="show-messages">
            <a href="#" onclick="show_messages(this); return false;">
              Show <xsl:value-of select="$messageCount"/> messages.
            </a>
          </li>
          <li class="hide-messages">
            <a href="#" onclick="hide_messages(this); return false;">
              Hide <xsl:value-of select="$messageCount"/> messages.
            </a>
          </li>
        </xsl:if>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="message|warning|error">
    <li>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name(.)"/>
        <xsl:value-of select="translate(@importance, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')"/>-importance
      </xsl:attribute>
      <xsl:value-of select="."/>
    </li>
  </xsl:template>



  <!-- String split template -->
  <xsl:template name="SplitString">
    <xsl:param name="source" select="''"/>
    <xsl:param name="separator" select="','"/>
    <xsl:if test="not($source = '' or $separator = '')">
      <xsl:variable name="head" select="substring-before(concat($source, $separator), $separator)"/>
      <xsl:variable name="tail" select="substring-after($source, $separator)"/>
      <part>
        <xsl:value-of select="$head"/>
      </part>
      <xsl:call-template name="SplitString">
        <xsl:with-param name="source" select="$tail"/>
        <xsl:with-param name="separator" select="$separator"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>


  <xsl:template match="projects" mode="summary">
    <!-- output summary -->
    <table>
      <tr>
        <th>Project</th>
        <th>Errors</th>
        <th>Warnings</th>
        <th>Messages</th>
      </tr>
      <xsl:apply-templates select="project" mode="summary"/>
    </table>

  </xsl:template>

  <xsl:template match="projects" mode="detail">
    <!-- ouput details -->
    <xsl:apply-templates select="project" mode="detail"/>

  </xsl:template>

  <xsl:template match="build">
    <!-- Output doc type the 'Mark of the web' which disabled prompting to run JavaScript from local HTML Files in IE -->
    <!-- NOTE: The whitespace around the 'Mark of the web' is important it must be exact -->
    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html>
<!-- saved from url=(0014)about:internet -->
]]>
    </xsl:text>
    <html>
      <head>
        <meta content="en-us" http-equiv="Content-Language"/>
        <meta content="text/html; charset=utf-8" http-equiv="Content-Type"/>
        <link type="text/css" rel="stylesheet" href="_BuildReport_Files/BuildReport.css"/>
        <title>
          Build report
        </title>

        <script type="text/javascript" language="javascript">
          <xsl:text disable-output-escaping="yes">
          <![CDATA[
function show_messages(e)
{
  var parent = e.parentElement.parentElement;
  parent.className = parent.className.replace(' collapsed', '');
}

function hide_messages(e)
{
  var parent = e.parentElement.parentElement;
  parent.className = parent.className.replace(' collapsed', '') + ' collapsed';
}

          ]]>
        </xsl:text>
        </script>
      </head>
      <body>
        <xsl:variable name="buildXML">
          <xsl:apply-templates select="self::node()" mode="buildXML"/>
        </xsl:variable>
        <h1 >
          Build Report - <xsl:value-of select="msxsl:node-set($buildXML)/projects/project[@is-solution]/@name"/>
        </h1>

        <div id="content">
          <h2>Overview</h2>

          <div id="overview">
            <xsl:apply-templates select="msxsl:node-set($buildXML)/*" mode="summary"/>
          </div>

          <h2>Projects</h2>

          <div id="messages">
            <xsl:apply-templates select="msxsl:node-set($buildXML)/*" mode="detail"/>
          </div>
        </div>

      </body>
    </html>

  </xsl:template>

</xsl:stylesheet>
