using DBMT_Core.Utils;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMT_Core.Common
{
    public class VertexBufferTxtFile
    {
        /// <summary>
        /// 从txt文件里识别到的d3d11ElementList
        /// </summary>
        public List<D3D11Element> D3d11ElementList { get; set; } = [];

        public Dictionary<string, D3D11Element> ElementName_D3D11Element_Dict { get; set; } = new Dictionary<string, D3D11Element>();
        public string Stride { get; set; } = "";
        public string FirstVertex { get; set; } = "";
        public string VertexCount { get; set; } = "";
        public string Topology { get; set; } = "";

        public string[] VBTxtFileLines = [];

        public VertexBufferTxtFile() {
           
        }

        /// <summary>
        /// 只负责处理属性的解析和读取
        /// </summary>
        public void ParseAttributes()
        {
            LOG.Info("ParseAttributes::Start");
            foreach (string Line in VBTxtFileLines)
            {
                //如果没遇到element，说明处理的是属性，那就一直处理属性，直到遇到后就不会再触发这里了
                if (Line.StartsWith("stride: "))
                {
                    Stride = Line.Substring("stride: ".Length);
                    LOG.Info("Stride: " + Stride);
                }
                else if (Line.StartsWith("first vertex: "))
                {
                    FirstVertex = Line.Substring("first vertex: ".Length);
                    LOG.Info("FirstVertex: " + FirstVertex);

                }
                else if (Line.StartsWith("vertex count: "))
                {
                    VertexCount = Line.Substring("vertex count: ".Length);
                    LOG.Info("VertexCount: " + VertexCount);

                }
                else if (Line.StartsWith("topology: "))
                {
                    Topology = Line.Substring("topology: ".Length);
                    LOG.Info("Topology: " + Topology);

                }
                else if (Line.StartsWith("element["))
                {
                    break;
                }
            }

            LOG.NewLine("ParseAttributes::End");
        }

        public void ParseElementList()
        {
            LOG.Info("ParseElementList::Start");
            //解析前先清空当前的ElementList列表，防止其它问题。
            this.D3d11ElementList.Clear();

            bool MeetElement = false;
            D3D11Element tmpD3D11Element = new D3D11Element();

            foreach (string Line in VBTxtFileLines)
            {
                if (!MeetElement)
                {
                    if (Line.StartsWith("element["))
                    {
                        MeetElement = true;
                        continue;
                    }
                }
                else
                {
                    string TrimLowerLine = Line.Trim().ToLower();
                    if (TrimLowerLine.StartsWith("SemanticName".ToLower()))
                    {
                        tmpD3D11Element.SemanticName = Line.Trim().Substring("SemanticName".Length + 2);
                        LOG.Info("SemanticName: " + tmpD3D11Element.SemanticName);
                    }
                    else if (TrimLowerLine.StartsWith("SemanticIndex".ToLower()))
                    {
                        tmpD3D11Element.SemanticIndex = int.Parse(Line.Trim().Substring("SemanticIndex".Length + 2));
                        LOG.Info("SemanticIndex: " + tmpD3D11Element.SemanticIndex);
                    }
                    else if (TrimLowerLine.StartsWith("Format".ToLower()))
                    {
                        tmpD3D11Element.Format = Line.Trim().Substring("Format".Length + 2);
                        LOG.Info("Format: " + tmpD3D11Element.Format);
                    }
                    else if (Line.StartsWith("element["))
                    {
                        //计算ElementName
                        if (tmpD3D11Element.SemanticIndex == 0)
                        {
                            tmpD3D11Element.ElementName = tmpD3D11Element.SemanticName;
                        }
                        else
                        {
                            tmpD3D11Element.ElementName = tmpD3D11Element.SemanticName + tmpD3D11Element.SemanticIndex.ToString();
                        }

                        //计算ByteWidth
                        tmpD3D11Element.ByteWidth = DBMTFormatUtils.GetByteWidthFromFormat(tmpD3D11Element.Format);

                        //加入列表
                        this.D3d11ElementList.Add(tmpD3D11Element);

                        //加入字典
                        this.ElementName_D3D11Element_Dict[tmpD3D11Element.ElementName] = tmpD3D11Element;

                        //清理
                        tmpD3D11Element = new D3D11Element();
                    }
                    else if (Line.StartsWith("vertex-data:"))
                    {
                        break;
                    }
                }
            }

            LOG.NewLine("ParseElementList::End");

        }

        /// <summary>
        /// 只读取属性和Element列表
        /// 其实这里和读取fmt文件是一样的操作
        /// </summary>
        /// <param name="VBTxtFilePath"></param>
        public VertexBufferTxtFile(string VBTxtFilePath) {
            this.VBTxtFileLines = File.ReadAllLines(VBTxtFilePath);
            this.ParseAttributes();
            this.ParseElementList();



        }

    }
}
