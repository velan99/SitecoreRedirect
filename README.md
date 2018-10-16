

Simple redirect module for the Sitecore 9 and Sitecore 8. It has the following options for the redirect
<ol>
	<li>Sitecore Internal Items Redirect</li>
	<li>Regex Options</li>
	<li>Broken Links</li>
</ol>
It Supports 3 type of redirection
<ol>
	<li>Permanent Redirect (301)</li>
	<li>Temporary Redirect (302)</li>
	<li>Server Transfer</li>
</ol>
How to Use the Redirect Module

1. Download and install the Redirect Module from the market place.
2. Add these entry to your Site config file.

redirectSettingsId="{23F429BA-9BBF-4FDD-9BCD-1E7E46A0D029}"

Ex. <site name="website" enableTracking="true" virtualFolder="/" physicalFolder="/" rootPath="/sitecore/content" redirectSettingsId="{23F429BA-9BBF-4FDD-9BCD-1E7E46A0D029}" startItem="/home" language="en" database="web" domain="extranet" allowDebug="true" cacheHtml="true" htmlCacheSize="50MB" registryCacheSize="0" viewStateCacheSize="0" xslCacheSize="25MB" filteredItemsCacheSize="10MB" enablePreview="true" enableWebEdit="true" enableDebugger="true" disableClientData="false" cacheRenderingParameters="true" renderingParametersCacheSize="10MB" enableItemLanguageFallback="false" enableFieldLanguageFallback="false" role:require="Standalone or Reporting or ContentManagement or ContentDelivery" />

<strong>How to Use the Redirect Module</strong>

1. Internal Item Redirect
<ol>
	<li>Add the new redirect item</li>
</ol>


Map the target Item and Redirect Url

2. Regex


For Ex. ^/test3(/.*)?$ - /test$1

<strong>Target Url : </strong>http://sitecoresite/test3/home4?test=1

<strong>Redirect Url :</strong>  http://sitecoresite/test/home4?test=1

3. Redirect Map

 

For the more information <a href="https://github.com/velan99/SitecoreRedirect" target="_blank" rel="noopener">Click here</a>.
