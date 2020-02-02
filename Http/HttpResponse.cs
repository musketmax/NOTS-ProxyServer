﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServer.Http
{
    public class HttpResponse : HttpMessage
    {
        public HttpResponse(string firstLine, List<HttpHeader> headers, byte[] body, byte[] responseInBytes) : base(firstLine, headers, body, responseInBytes) { }

        public static HttpResponse ParseToHTTPResponse(byte[] responseBytes)
        {
            string responseString = Encoding.UTF8.GetString(responseBytes);
            List<string> responseLines = ToLines(responseString);

            string firstLine = responseLines[0];
            List<HttpHeader> headers = ReadHeaders(responseLines);
            byte[] body = ReadBody(responseString);

            if (headers.Count() > 0) return new HttpResponse(firstLine, headers, body, responseBytes);

            return null;
        }

        public static HttpResponse Return407()
        {
            string firstLine = "HTTP/1.1 407 Proxy Authentication Required";

            List<HttpHeader> headers = new List<HttpHeader>
            {
                new HttpHeader("Connection", "close"),
                new HttpHeader("Content-Type", "text/html; charset=utf-8"),
                new HttpHeader("Date", DateTime.Now.ToUniversalTime().ToString("r"))
            };

            byte[] body = Encoding.UTF8.GetBytes("<html><h1>The server understood the request but refuses to authorize it. Please authorize and try again.</h1></html>");

            return new HttpResponse(firstLine, headers, body, new byte[0]);
        }

        public static HttpResponse GetSafePlaceholderImageResponse()
        {
            string firstLine = "HTTP/1.1 200 Ok";
            List<HttpHeader> headers = new List<HttpHeader>
            {
                new HttpHeader("Connection", "close"),
                new HttpHeader("Content-Type", "image/svg+xml"),
                new HttpHeader("Date", DateTime.Now.ToUniversalTime().ToString("r"))
            };

            byte[] body = Encoding.UTF8.GetBytes("<svg   xmlns:dc='http://purl.org/dc/elements/1.1/'   xmlns:cc='http://creativecommons.org/ns#'   xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'   xmlns:svg='http://www.w3.org/2000/svg'   xmlns='http://www.w3.org/2000/svg'   xmlns:sodipodi='http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd'   xmlns:inkscape='http://www.inkscape.org/namespaces/inkscape'   width='1010.4'   height='1010.4002'   id='svg2'   sodipodi:version='0.32'   inkscape:version='0.48.4 r9939'   version='1.0'   sodipodi:docname='Ajax.svg'   inkscape:output_extension='org.inkscape.output.svg.inkscape'>  <defs     id='defs4'>    <inkscape:perspective       sodipodi:type='inkscape:persp3d'       inkscape:vp_x='0 : 300.5 : 1'       inkscape:vp_y='0 : 1000 : 0'       inkscape:vp_z='601 : 300.5 : 1'       inkscape:persp3d-origin='300.5 : 200.33333 : 1'       id='perspective3380' />    <inkscape:perspective       id='perspective3990'       inkscape:persp3d-origin='375 : 250 : 1'       inkscape:vp_z='750 : 375 : 1'       inkscape:vp_y='0 : 1000 : 0'       inkscape:vp_x='0 : 375 : 1'       sodipodi:type='inkscape:persp3d' />  </defs>  <sodipodi:namedview     id='base'     pagecolor='#ffffff'     bordercolor='#666666'     borderopacity='1.0'     gridtolerance='10000'     guidetolerance='10'     objecttolerance='10'     inkscape:pageopacity='0.0'     inkscape:pageshadow='2'     inkscape:zoom='0.44192573'     inkscape:cx='451.12266'     inkscape:cy='489.66728'     inkscape:document-units='px'     inkscape:current-layer='layer1'     width='601px'     height='601px'     inkscape:window-width='1280'     inkscape:window-height='1001'     inkscape:window-x='-8'     inkscape:window-y='-8'     showgrid='false'     fit-margin-top='0.2'     fit-margin-left='0.2'     fit-margin-right='0.2'     fit-margin-bottom='0.2'     inkscape:window-maximized='1'     showguides='true'     inkscape:guide-bbox='true' />  <metadata     id='metadata7'>    <rdf:RDF>      <cc:Work         rdf:about=''>        <dc:format>image/svg+xml</dc:format>        <dc:type           rdf:resource='http://purl.org/dc/dcmitype/StillImage' />        <dc:title></dc:title>      </cc:Work>    </rdf:RDF>  </metadata>  <g     inkscape:label='Layer 1'     inkscape:groupmode='layer'     id='layer1'     transform='translate(1515.529,3241.075)'>    <rect       style='color:#000000;fill:#000000;fill-opacity:1;fill-rule:nonzero;stroke:none;stroke-width:0.8735441;marker:none;visibility:visible;display:inline;overflow:visible'       id='rect3653'       width='0'       height='27.202072'       x='578.73834'       y='298.1684'       rx='249.48199'       ry='27.202072' />    <path       inkscape:connector-curvature='0'       id='path3940'       d='m -801.1513,-2230.8749 295.8223,-295.8221 -10e-5,-418.3556 -295.822,-295.8224 -418.3557,3e-4 -295.8222,295.8221 2e-4,418.3558 295.822,295.822 z'       style='fill:#000000;fill-opacity:1;stroke:none' />    <path       inkscape:connector-curvature='0'       id='path3942'       d='m -803.2223,-2235.875 292.8933,-292.8932 -10e-5,-414.2135 -292.8931,-292.8935 -414.2135,3e-4 -292.8933,292.8932 2e-4,414.2136 292.8931,292.8932 z'       style='fill:#ffffff;fill-opacity:1;stroke:none' />    <path       style='fill:#af1e2d;fill-opacity:1;stroke:none'       d='m -1207.0806,-2260.875 -278.2484,-278.2483 10e-5,-393.5032 278.2482,-278.2486 393.50294,4e-4 278.24876,278.2481 -2.9e-4,393.5033 -278.24837,278.2484 z'       id='path3944'       inkscape:connector-curvature='0' />    <path       inkscape:connector-curvature='0'       d='m -640.91616,-2809.0169 c 0,30.3744 -15.19929,47.9129 -43.24646,47.9129 l -49.10072,0 0,-94.6742 49.10072,0 c 25.72,0 43.24646,18.702 43.24646,46.7613 m 46.73716,0 c 0,-56.1185 -35.05288,-93.5228 -87.65645,-93.5228 l -99.34078,0 0,333.1358 47.91289,0 0,-146.1144 51.42789,0 c 56.11855,0 87.65645,-32.7256 87.65645,-93.4986 m -285.21074,72.4694 c 0,38.5678 -2.32717,58.4456 -10.53286,85.3171 -8.18142,28.0472 -21.02929,40.9193 -40.89504,40.9193 -21.05346,0 -33.91351,-12.8721 -42.07072,-40.9193 -7.02995,-26.8715 -10.53283,-47.9249 -10.53283,-84.1657 0,-37.4042 2.32712,-59.6093 10.53283,-85.3293 8.15721,-28.0351 21.01726,-40.9071 42.07072,-40.9071 19.86575,0 32.71362,12.872 40.89504,42.0828 7.02999,24.5443 10.53286,45.5857 10.53286,83.0022 m 45.5857,0 c 0,-50.2765 -4.69068,-81.8265 -18.70213,-114.5644 -16.36287,-40.895 -39.73147,-57.27 -78.31147,-57.27 -38.56784,0 -61.96072,16.375 -79.48706,56.1186 -14.0115,31.5379 -18.7144,65.4393 -18.7144,115.7158 0,51.4278 4.7029,82.9657 18.7144,115.7035 16.36278,39.7436 39.75556,57.4761 79.48706,57.4761 37.4043,0 61.9486,-17.7325 78.31147,-57.4761 14.01145,-31.55 18.70213,-64.2757 18.70213,-115.7035 m -220.94716,-121.57 0,-44.4222 -169.4828,0 0,44.4222 60.785,0 0,288.7136 47.9129,0 0,-288.7136 60.7849,0 z m -185.882,197.5421 c 0,-47.925 -28.035,-84.1657 -92.323,-121.5579 -24.5442,-14.0357 -31.55,-23.3928 -31.55,-39.7556 0,-22.205 17.5265,-38.568 39.7315,-38.568 21.0293,0 37.4043,15.1993 38.5679,38.568 l 45.5736,0 c -1.1636,-50.2644 -37.3801,-86.493 -84.1415,-86.493 -47.925,0 -85.3293,38.568 -85.3293,86.493 0,29.235 14.0114,54.9427 40.9072,73.645 67.8028,45.5735 49.1007,32.7256 56.1185,39.7435 19.8536,15.1872 26.8715,29.1987 26.8715,47.925 0,26.8836 -21.0415,50.2643 -46.7494,50.2643 -32.7256,0 -46.7613,-29.235 -46.7613,-57.2942 l -46.7615,0 c 0,54.9428 35.0772,104.2374 93.5228,104.2374 51.4279,0 92.323,-43.4403 92.323,-97.2075'       style='fill:#ffffff;fill-opacity:1;fill-rule:evenodd;stroke:none'       id='path3338' />  </g></svg>");

            return new HttpResponse(firstLine, headers, body, new byte[0]);
        }
    }
}
